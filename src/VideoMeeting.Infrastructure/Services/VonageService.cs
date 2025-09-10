using VideoMeeting.Application.Common.Interfaces;
using VideoMeeting.Application.Common.Models;
using VideoMeeting.Shared.Configuration;
using Vonage;
using Vonage.Common.Exceptions;
using Vonage.Request;
using Vonage.Server;
using Vonage.Video;
using Vonage.Video.Archives;
using Vonage.Video.Archives.CreateArchive;
using Vonage.Video.Archives.GetArchive;
using Vonage.Video.Archives.GetArchives;
using Vonage.Video.Archives.StopArchive;
using Vonage.Video.Authentication;
using Vonage.Video.Sessions;
using Vonage.Video.Sessions.CreateSession;
using Vonage.Video.Signaling;
using Vonage.Video.Signaling.SendSignals;

namespace VideoMeeting.Infrastructure.Services;

public class VonageService : IVonageService
{
    private readonly VonageConfiguration _vonageConfig;
    private readonly IVideoTokenGenerator _tokenGenerator;
    private readonly Credentials _credentials;
    private readonly IVideoClient _videoClient;
    private readonly VonageClient _client;
    private readonly SemaphoreSlim _archiveSemaphore = new SemaphoreSlim(1, 1);

    public VonageService(VonageConfiguration vonageConfig, IVideoTokenGenerator tokenGenerator)
    {
        _vonageConfig = vonageConfig;

        if (string.IsNullOrEmpty(_vonageConfig.ApplicationId) || string.IsNullOrEmpty(_vonageConfig.PrivateKeyPath))
            throw new InvalidOperationException("Vonage configuration is missing required values");

        var applicationId = _vonageConfig.ApplicationId;
        var privateKeyPath = Path.Combine(Environment.CurrentDirectory, _vonageConfig.PrivateKeyPath);
        _credentials = Credentials.FromAppIdAndPrivateKeyPath(applicationId, privateKeyPath);
        _tokenGenerator = tokenGenerator;
        _client = new VonageClient(_credentials);
        _videoClient = _client.VideoClient;
    }

    public async Task<ApiResponse<string>> CreateSession()
    {
        try
        {
            // var videoClient = _client.VideoClient;
            var request = CreateSessionRequest.Build()
                .WithLocation(string.Empty)
                .WithMediaMode(MediaMode.Routed)
                .WithArchiveMode(ArchiveMode.Manual)
                .Create();
            // Send the request to the API
            var response = await _videoClient.SessionClient.CreateSessionAsync(request);
            return response.Match(
                success => new ApiResponse<string>
                {
                    Success = true,
                    Data = success.SessionId
                },
                failure => new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Failed to create session: {failure.GetFailureMessage()} /n{failure.ToException()}"
                }
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>
            {
                Success = false,
                Message = $"Exception occurred: {ex.Message}"
            };
        }
    }

    public ApiResponse<string> GenerateToken(string sessionId, string role = "publisher", string userName = "Unknown")
    {
        var expireTime = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds();
        var tokenRole = role.ToLower() switch
        {
            "moderator" => Role.Moderator,
            "subscriber" => Role.Subscriber,
            _ => Role.Publisher
        };

        try
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Session ID cannot be null or empty."
                };
            }

            // Define the additional claims with expiration time
            var claims = TokenAdditionalClaims.Parse(
                sessionId,
                role: tokenRole,
                claims: new Dictionary<string, object>
                {
                    { "exp", expireTime }, // Expiry time of 60 minutes
                    { "connection_data", userName }
                }
            );
            // Generate the token
            var tokenResult = _tokenGenerator.GenerateToken(_credentials, claims);

            // Handle the token generation result
            return tokenResult.Match(
                success => new ApiResponse<string>
                {
                    Success = true,
                    Data = success.Token // Extract token from successful result
                },
                failure => new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Failed to generate token: {failure.GetFailureMessage()} \n{failure.ToException()}"
                }
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>
            {
                Success = false,
                Message = $"Exception occurred: {ex.Message}"
            };
        }
    }


    public async Task<ApiResponse<string>> StartRecordingAsync_1(string sessionId,
        CancellationToken cancellationToken = default)
    {
        await _archiveSemaphore.WaitAsync();
        try
        {
            Guid applicationId = new Guid(_vonageConfig.ApplicationId);
            var request = CreateArchiveRequest.Build()
                .WithApplicationId(applicationId)
                .WithSessionId(sessionId)
                .WithOutputMode(OutputMode.Composed)
                .WithResolution(RenderResolution.HighDefinitionLandscape)
                .Create();

            var archiveResponse = await _videoClient.ArchiveClient.CreateArchiveAsync(request);

            return archiveResponse.Match(
                success => new ApiResponse<string>()
                {
                    Success = true,
                    Data = success.Id
                },
                failure => new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Failed to create archive: {failure.GetFailureMessage()} /n{failure.ToException()}"
                }
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>()
            {
                Success = false,
                Message = $"Exception:{ex.Message}, Inner Exception:{ex?.InnerException?.Message}"
            };
        }
        finally
        {
            _archiveSemaphore.Release();
        }
    }

    public async Task<ApiResponse<Archive>> StartRecordingAsync1(string sessionId,
        CancellationToken cancellationToken = default)
    {
        const int maxRetries = 3;
        var applicationId = Guid.Parse(_vonageConfig.ApplicationId);

        var request = CreateArchiveRequest.Build()
            .WithApplicationId(applicationId)
            .WithSessionId(sessionId)
            .WithOutputMode(OutputMode.Composed)
            .WithResolution(RenderResolution.HighDefinitionLandscape)
            .Create();

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var archiveResponse = await _videoClient.ArchiveClient.CreateArchiveAsync(request);

                return archiveResponse.Match(
                    success => new ApiResponse<Archive>()
                    {
                        Success = true,
                        Data = success
                    },
                    failure => new ApiResponse<Archive>()
                    {
                        Success = false,
                        Message = $"Failed to create archive: {failure.GetFailureMessage()} /n{failure.ToException()}"
                    }
                );
            }
            catch (VonageException ex) when (ex.Message.Contains("409"))
            {
                var recordedData = await GetArchiveBySessionIdAsync(sessionId);
                if (recordedData.Success == true)
                {
                    var activeArchive = recordedData.Data.Items.FirstOrDefault(x => x.Status == "started");

                    if (!string.IsNullOrEmpty(activeArchive.Id))
                    {
                        return new ApiResponse<Archive>()
                        {
                            Success = true,
                            Data = activeArchive
                        };
                    }
                }
                else
                {
                    return new ApiResponse<Archive>()
                    {
                        Success = false,
                        Message = $"Failed to retrieve existing archives: {recordedData.Message}"
                    };
                }


                if (attempt < maxRetries)
                    await Task.Delay(1000 * attempt); // Exponential backoff delay
            }
            catch (Exception ex)
            {
                return new ApiResponse<Archive>()
                {
                    Success = false,
                    Message = $"Exception:{ex.Message}, Inner Exception:{ex?.InnerException?.Message}"
                };
            }
        }

        return new ApiResponse<Archive>()
        {
            Success = false,
            Message = "Max retry attempts reached. Could not start archive."
        };
    }

    public async Task<ApiResponse<Archive>> StartRecordingAsync(string sessionId,
        CancellationToken cancellationToken = default)
    {
        const int maxRetries = 3;
        var applicationId = Guid.Parse(_vonageConfig.ApplicationId);

        var request = CreateArchiveRequest.Build()
            .WithApplicationId(applicationId)
            .WithSessionId(sessionId)
            .WithOutputMode(OutputMode.Composed)
            .WithResolution(RenderResolution.HighDefinitionLandscape)
            .Create();

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var archiveResponse = await _videoClient.ArchiveClient.CreateArchiveAsync(request);

                return await archiveResponse.Match(
                    success => Task.FromResult(new ApiResponse<Archive>
                    {
                        Success = true,
                        Data = success
                    }),
                    async failure =>
                    {
                        // Try to get existing active archive on failure
                        var recordedData = await GetArchiveBySessionIdAsync(sessionId);

                        if (recordedData.Success)
                        {
                            var activeArchive = recordedData.Data.Items.FirstOrDefault(x => x.Status == "started");

                            if (!string.IsNullOrEmpty(activeArchive.Id))
                            {
                                return new ApiResponse<Archive>
                                {
                                    Success = true,
                                    Data = activeArchive
                                };
                            }
                        }

                        return new ApiResponse<Archive>
                        {
                            Success = false,
                            Message =
                                $"Failed to create archive: {failure.GetFailureMessage()} /n{failure.ToException()}"
                        };
                    }
                );
            }
            catch (VonageException ex) when (ex.Message.Contains("409"))
            {
                var recordedData = await GetArchiveBySessionIdAsync(sessionId);

                if (recordedData.Success)
                {
                    var activeArchive = recordedData.Data.Items.FirstOrDefault(x => x.Status == "started");

                    if (!string.IsNullOrEmpty(activeArchive.Id))
                    {
                        return new ApiResponse<Archive>
                        {
                            Success = true,
                            Data = activeArchive
                        };
                    }
                }

                if (attempt < maxRetries)
                    await Task.Delay(1000 * attempt, cancellationToken); // Exponential backoff
            }
            catch (Exception ex)
            {
                return new ApiResponse<Archive>
                {
                    Success = false,
                    Message = $"Exception: {ex.Message}, Inner Exception: {ex?.InnerException?.Message}"
                };
            }
        }

        return new ApiResponse<Archive>
        {
            Success = false,
            Message = "Max retry attempts reached. Could not start archive."
        };
    }


    public async Task<ApiResponse<object>> StopRecordingAsync(string archiveId,
        CancellationToken cancellationToken = default)
    {
        await _archiveSemaphore.WaitAsync();
        try
        {
            Guid applicationId = new Guid(_vonageConfig.ApplicationId);
            Guid guidArchiveId = new Guid(archiveId);
            var request = StopArchiveRequest.Build()
                .WithApplicationId(applicationId)
                .WithArchiveId(guidArchiveId)
                .Create();

            var archiveResponse = await _videoClient.ArchiveClient.StopArchiveAsync(request);

            return archiveResponse.Match(
                success => new ApiResponse<object>()
                {
                    Success = true,
                    Data = success
                },
                failure => new ApiResponse<Object>
                {
                    Success = false,
                    Message = $"Failed to create archive: {failure.GetFailureMessage()} /n {failure.ToException()}"
                }
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse()
            {
                Success = false,
                Message = $"Exception:{ex.Message}, Inner Exception:{ex?.InnerException?.Message}"
            };
        }
        finally
        {
            _archiveSemaphore.Release();
        }
    }

    public async Task<ApiResponse<Archive>> GetArchiveAsync(string archiveId)
    {
        try
        {
            Guid applicationId = new Guid(_vonageConfig.ApplicationId);
            Guid guidArchiveId = new Guid(archiveId);
            var request = GetArchiveRequest.Build()
                .WithApplicationId(applicationId)
                .WithArchiveId(guidArchiveId)
                .Create();

            var response = await _videoClient.ArchiveClient.GetArchiveAsync(request);
            return response.Match(
                success => new ApiResponse<Archive>()
                {
                    Success = true,
                    Data = success
                },
                failure => new ApiResponse<Archive>()
                {
                    Success = false,
                    Message = $"Failed to get archive: {failure.GetFailureMessage()} /n{failure.ToException()}"
                }
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<Archive>()
            {
                Success = false,
                Message = $"Exception:{ex.Message}, Inner Exception:{ex?.InnerException?.Message}"
            };
        }
    }


    public async Task<ApiResponse<object>> SignalHostDisconnection(string sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Guid applicationId = new Guid(_vonageConfig.ApplicationId);

            var signalData = new SignalContent("hostDisconnected", "The host has ended the session");

            var request = SendSignalsRequest.Build()
                .WithApplicationId(applicationId)
                .WithSessionId(sessionId)
                .WithContent(signalData)
                .Create();
            var signalResponse = await _videoClient.SignalingClient.SendSignalsAsync(request);
            return signalResponse.Match(
                success => new ApiResponse<object>()
                {
                    Success = true,
                    Message = "Signal sent successfully",
                    Data = success
                },
                failure => new ApiResponse<object>()
                {
                    Success = true,
                    Message = $"Failed to create archive: {failure.GetFailureMessage()}\n{failure.ToException()}"
                }
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<object>()
            {
                Success = true,
                Message = $"{ex.Message}\n{ex?.InnerException?.Message}"
            };
        }
    }

    public async Task<ApiResponse<GetArchivesResponse>> GetArchiveBySessionIdAsync(string sessionId)
    {
        try
        {
            Guid applicationId = new Guid(_vonageConfig.ApplicationId);
            var request = GetArchivesRequest.Build()
                .WithApplicationId(applicationId)
                .WithSessionId(sessionId)
                .Create();

            var response = await _videoClient.ArchiveClient.GetArchivesAsync(request);
            return response.Match(
                success => new ApiResponse<GetArchivesResponse>()
                {
                    Success = true,
                    Data = success
                },
                failure => new ApiResponse<GetArchivesResponse>()
                {
                    Success = false,
                    Message = $"Failed to create archive: {failure.GetFailureMessage()} /n{failure.ToException()}"
                }
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<GetArchivesResponse>()
            {
                Success = false,
                Message = $"Exception:{ex.Message}, Inner Exception:{ex?.InnerException?.Message}"
            };
        }
    }


    // public async Task<RecordingInfoDto> GetRecordingInfoAsync(string recordingId,
    //     CancellationToken cancellationToken = default)
    // {
    //     try
    //     {
    //         var archive = await _openTok.GetArchiveAsync(recordingId);
    //
    //         return new RecordingInfoDto(
    //             archive.Id.ToString(),
    //             MapArchiveStatus(archive.Status),
    //             (int)archive.Duration,
    //             archive.Size,
    //             archive.Url,
    //             DateTimeOffset.FromUnixTimeSeconds(archive.CreatedAt).DateTime,
    //             DateTimeOffset.FromUnixTimeSeconds(archive.CreatedAt).DateTime
    //         );
    //     }
    //     catch (Exception ex)
    //     {
    //         throw new InvalidOperationException($"Failed to get recording info: {ex.Message}", ex);
    //     }
    // }

    // private static RecordingStatus MapArchiveStatus(string status)
    // {
    //     return status switch
    //     {
    //         ArchiveStatus.STARTED => RecordingStatus.Recording,
    //         ArchiveStatus.STOPPED => RecordingStatus.Processing,
    //         ArchiveStatus.AVAILABLE => RecordingStatus.Available,
    //         ArchiveStatus.FAILED => RecordingStatus.Failed,
    //         ArchiveStatus.DELETED => RecordingStatus.Deleted,
    //         _ => RecordingStatus.Failed
    //     };
    // }
}
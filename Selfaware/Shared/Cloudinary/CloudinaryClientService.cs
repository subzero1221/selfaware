using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using Selfaware.Shared.Models;

namespace Selfaware.Shared.Cloudinary
{
    public class CloudinaryClientService : IImageService
    {
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;
        private readonly CloudinarySettings _cloudinarySettings;
        public CloudinaryClientService(IOptions<CloudinarySettings> cloudinarySettings)
        {

            _cloudinarySettings = cloudinarySettings.Value;

            if (string.IsNullOrWhiteSpace(_cloudinarySettings.CloudName))
            {
                throw new InvalidOperationException("Cloudinary settings failed to bind! Check appsettings.json key names.");
            }

            var account = new Account(
            _cloudinarySettings.CloudName,
            _cloudinarySettings.ApiKey,
            _cloudinarySettings.ApiSecret
        );
            _cloudinary = new CloudinaryDotNet.Cloudinary(account);
        }

        public ServiceResult<UploadSignatureDto>GenerateUploadSignature(string folder)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            var parameters = new Dictionary<string, object>
        {
           { "folder", folder },
            { "timestamp", timestamp }
        };

           

            string signature = _cloudinary.Api.SignParameters(parameters);

            var signatureData = new UploadSignatureDto(
                Signature: signature,
                Timestamp: timestamp,
                ApiKey: _cloudinarySettings.ApiKey,
                CloudName: _cloudinarySettings.CloudName,
                Folder: folder
            );

            return ServiceResult<UploadSignatureDto>.Ok(signatureData, "signature given succesfully");
        }

        public async Task <ServiceResult<string>>DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId)) return ServiceResult<string>.Failed("Image not found");

            var deleteParams = new DeletionParams(publicId);
            await _cloudinary.DestroyAsync(deleteParams);

            return ServiceResult<string>.Ok(publicId, "Image Deleted succesfully");
        }

    }
    
 }

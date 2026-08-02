using Selfaware.Shared.Models;

namespace Selfaware.Shared.Cloudinary
{
    public interface IImageService
    {
        Task <ServiceResult<string>> DeleteImageAsync(string publicId);
        ServiceResult<UploadSignatureDto> GenerateUploadSignature(string folder);
    }


    public record UploadSignatureDto(
    string Signature,
    string Timestamp,
    string ApiKey,
    string CloudName,
    string Folder
);
}

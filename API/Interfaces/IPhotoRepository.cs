using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;

namespace API.Interfaces
{
    public interface IPhotoRepository
    {
        void RemovePhoto (Photo photo);
        Task<Photo?> GetPhotoById (int photoId);
        Task<IEnumerable<PhotoForApprovalDto>> GetUnApprovedPhotos ();
        Task<IEnumerable<PhotoForApprovalDto>> GetApprovedPhotos ();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PhotoRepository(DataContext context,IMapper mapper): IPhotoRepository 
    {
        public async Task<Photo?> GetPhotoById(int photoId)
        {
            return await context.Photos
            .IgnoreQueryFilters()
            .FirstAsync(x=>x.Id==photoId);
        }
        public void RemovePhoto(Photo photo)
        {
            context.Photos.Remove(photo);
        }

        public async Task<IEnumerable<PhotoForApprovalDto>> GetUnApprovedPhotos()
        {
            var query = context.Photos
            .IgnoreQueryFilters()
                .Where(x=>x.IsApproved == false)
                .AsQueryable();
            return await query.ProjectTo<PhotoForApprovalDto>(mapper.ConfigurationProvider).ToListAsync();
        }
        public async Task<IEnumerable<PhotoForApprovalDto>> GetApprovedPhotos()
        {
            var query = context.Photos
            .IgnoreQueryFilters()
                .Where(x=>x.IsApproved == true)
                .AsQueryable();
            return await query.ProjectTo<PhotoForApprovalDto>(mapper.ConfigurationProvider).ToListAsync();
        }
    }
}
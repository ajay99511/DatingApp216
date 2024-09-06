using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AdminController(UserManager<AppUser> userManager,IUnitOfWork unitOfWork,IPhotoService photoService) : BaseApiController
{
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles()
    {
        var users = await userManager.Users
        .OrderBy(x=>x.UserName)
        .Select(x=> new{
            x.Id,
            Username = x.UserName,
            Roles = x.UserRoles.Select(x=>x.Role.Name).ToList()
        })
        .ToListAsync();
        return Ok(users);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{username}")]
    public async Task<ActionResult> EditRoles(string username, string roles)
    {
        if(string.IsNullOrEmpty(roles)) return BadRequest("You have select atleast one role");
        var SelectedRoles = roles.Split(",").ToArray();

        var user = await userManager.FindByNameAsync(username);
        if (user == null) return BadRequest("Cannot find User");
        var userRoles = await userManager.GetRolesAsync(user);

        // var rolesToAdd = SelectedRoles.Where(role => !userRoles.Contains(role)).ToList();
        // var rolesToRemove = userRoles.Where(role => !SelectedRoles.Contains(role)).ToList();
        
        var result = await userManager.AddToRolesAsync(user,SelectedRoles.Except(userRoles));
        if(!result.Succeeded) return BadRequest("Failed to add roles");

        result = await userManager.RemoveFromRolesAsync(user,userRoles.Except(SelectedRoles));
        if(!result.Succeeded) return BadRequest("Failed to remove roles");

        return Ok(await userManager.GetRolesAsync(user));

    }

    [Authorize(Policy ="ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public async Task<ActionResult<IEnumerable<PhotoForApprovalDto>>> GetPhotosForModerator()
    {
        var photos = await unitOfWork.PhotoRepository.GetApprovedPhotos();
        return Ok(photos);
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-for-approval")]
    public async Task<ActionResult<IEnumerable<PhotoForApprovalDto>>> GetPhotosForAdminApproval()
    {
        var photos = await unitOfWork.PhotoRepository.GetUnApprovedPhotos();
        return Ok(photos);
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("approve")]
    public async Task<ActionResult<Photo>> ApprovePhoto(int photoId)
    {
        var photo = await unitOfWork.PhotoRepository.GetPhotoById(photoId);
        if(photo == null) return BadRequest("No Photo to Approve");
        photo.IsApproved = true;
        var user = await unitOfWork.UserRepository.GetUserByPhotoId(photoId);
        if(user == null) return BadRequest("Cannot get the user from database");
        if(!user.Photos.Any(p=>p.IsMain)) photo.IsMain = true;
        await unitOfWork.Complete();
        return Ok(photo);
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpDelete("reject")]
    public async Task<ActionResult> RejectPhoto(int photoId)
    {
        var photo = await unitOfWork.PhotoRepository.GetPhotoById(photoId);
        if(photo == null) return BadRequest("No Photo to Approve");
        if(photo.PublicId != null){
            var result =await photoService.DeletePhotoAsync(photo.PublicId);
            if(result.Result == "ok"){
                unitOfWork.PhotoRepository.RemovePhoto(photo);
            }
        }
        else{
            unitOfWork.PhotoRepository.RemovePhoto(photo);
        } 
        await unitOfWork.Complete();
        return Ok();
    }

    
    
}
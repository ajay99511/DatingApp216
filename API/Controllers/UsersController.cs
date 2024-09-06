using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController(IUnitOfWork unitOfWork,IMapper mapper,IPhotoService photoService) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
    {
        userParams.CurrentUsername = User.GetUsername();
        //var users = await context.Users.ToListAsync();
        var users = await unitOfWork.UserRepository.GetMembersAsync(userParams);
        //var usersToReturn = mapper.Map<IEnumerable<MemberDto>>(users);
        Response.AddPaginationHeader(users);
        return Ok(users); 
    }

    [HttpGet("{username}")]
    public async Task <ActionResult<MemberDto>> GetUser(string username)
    {
        //var user = await context.Users.FindAsync(id);
        var user = await unitOfWork.UserRepository.GetMemberAsync(username,(username == User.GetUsername()));
        if (user == null)
        {
            return NotFound();
        }
        return user; // mapper.Map<MemberDto>(user);
    }
    
    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        // var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // if(username == null) return BadRequest("No USername");
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
        if(user == null) return BadRequest("No User");
        mapper.Map(memberUpdateDto,user);
        if(await unitOfWork.Complete())
        {
            return NoContent();
        }
        return BadRequest("Failed to Update User");
    }
    
    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null) return BadRequest("cannot get user");
        var result = await photoService.AddPhotoAsync(file);
        if(result.Error !=null) return BadRequest(result.Error.Message);
        var photo = new Photo{
            Url=result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };
        // if(user.Photos.Count ==0) photo.IsMain=true;
        user.Photos.Add(photo);
        if(await unitOfWork.Complete()) return CreatedAtAction(nameof(GetUser),new {username = user.UserName},mapper.Map<PhotoDto>(photo));
        //return mapper.Map<PhotoDto>(photo);
        return BadRequest("Problem Adding a Photo");

    }

    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId )  
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
        if(user==null) return BadRequest("Cannot find user");
        var photo = user.Photos.FirstOrDefault(p=>p.Id==photoId);
        if(photo==null || photo.IsMain) return BadRequest("cannot use this main photo");
        var currentMain = user.Photos.FirstOrDefault(p=>p.IsMain);
        if(currentMain != null) currentMain.IsMain = false;
        photo.IsMain = true;
        if(await unitOfWork.Complete()) return NoContent();
        return BadRequest("Problem while setting up th file");
    }

    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
        if(user==null) return BadRequest("Cannot find user");
        var photo = await unitOfWork.PhotoRepository.GetPhotoById(photoId);
        if(photo==null || photo.IsMain) return BadRequest("cannot use this main photo");
        if(photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);
            if(result.Error != null) return BadRequest(result.Error.Message);
        }
        user.Photos.Remove(photo);
        if(await unitOfWork.Complete()) return Ok();
        return BadRequest("Problem while deletion");
    }
}
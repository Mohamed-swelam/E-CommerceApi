using Core.DTOs.GeneralDto;
using Core.DTOs.user;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Interfaces.Services
{
    public interface IUserService
    {
        Task<GeneralResponse> GetProfileAsync(string userId);
        Task<GeneralResponse> UpdateProfileAsync(UpdateProfileDto dto, string userId);
    }
    
}

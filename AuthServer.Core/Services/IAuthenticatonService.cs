using AuthServer.Core.DTOs;
using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Core.Services
{
    public interface IAuthenticationService
    {
        Task<Response<TokenDto>> CreateTokenAsync(LoginDto loginDto);
        Task<Response<TokenDto>> CreateTokenByRefleshTokenAsync(string refleshToken);
        Task<Response<NoDataDto>> RevokeRefleshToken(string refleshToken);
        Response<ClientTokenDto> CreateTokenByClient(ClientLoginDto clientLoginDto);

    }
}

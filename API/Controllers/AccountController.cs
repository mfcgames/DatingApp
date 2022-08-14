using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTO;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
   
        public AccountController(DataContext context,ITokenService tokenService)
        {
            _tokenService = tokenService;
            _context = context;
        }
        
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            //Kullanıcının başka kullanıcı ile eşleşen ismini sorgular ve değiştirmesini ister..
            if(await UserExists(registerDto.Username)) return BadRequest("Username is taken");
            //Aşağıda kullanıcının ismini küçük duyarlı ve şifresini
            //sayısal ve harf değerlere çevirir kriptografi sağlar..
            using var hmac= new HMACSHA512();
            var user = new AppUser{
                UserName = registerDto.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt=hmac.Key
            };
            
            _context.Users.Add(user);
            
            await _context.SaveChangesAsync();
            
            return new UserDto{
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            }; 
        }

        //Kullanıcı Giriş Yapınca veri tabanıyla kullanıcıyı eşitler doğru ise 
        //giriş yapar lakin değil ise Uyarı Mesajı gönderir.
        [HttpPost("login")]
        public async Task<ActionResult<AppUser>> Login(LoginDto loginDto){
            var user = await _context.Users.SingleOrDefaultAsync(x=>x.UserName == loginDto.Username);

            if(user == null) return Unauthorized("Invalid Username");

            using var hmac =new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for(int i =0; i<computedHash.Length;i++){
                if(computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
            }

            return user;
        }

        //Mantık olarak kullanıcıların isminin aynı olmamasını sağlar..
        private async Task<bool>UserExists(string username)
        {
            return await _context.Users.AnyAsync(x=>x.UserName == username.ToString());
        }
    }
}
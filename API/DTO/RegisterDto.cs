using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTO
{
    public class RegisterDto
    {
        //Eklediğimiz Required Class'ı Giriş Yapan Kullanıcının Kullanıcı Adı
        //ve Şİfresini zorunlu kılar..
        [Required]
        public string UserName { get; set; }
        [Required]
        [StringLength(8,MinimumLength =4)]
        public string Password  { get; set; }
    }
}
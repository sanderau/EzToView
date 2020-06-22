using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EzToView.Models
{
    public class Upload
    {

        [Required(ErrorMessage ="Email is required.")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string email { get; set; }
       [Required(ErrorMessage ="A valid filepath is required")]
       public IFormFile file { get; set; }

       

    }
}

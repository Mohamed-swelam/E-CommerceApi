using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTOs.Product
{
    public class ProductImageDto
    {
        public IFormFile Image { get; set; }
    }
}

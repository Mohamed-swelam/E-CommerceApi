using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTOs.GeneralDto
{
    public class GeneralResponse
    {
        public bool IsSuccess { get; set; }
        public object? Data { get; set; }
    }
}

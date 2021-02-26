using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Phoenix.Extensions;

namespace Phoenix.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }
        

        [NotMapped]
        [Display(Name = "Select Image")]
        [DataType(DataType.Upload)]
        [MaxFileSize(2 * 1024 * 1024)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".doc", ".docx", ".xls", ".xlsx", ".pdf" })]
        public IFormFile FormFile { get; set; }
        public string FileName { get; set; }
        public byte[] FileData { get; set; }
        public string FilePath { get; set; }

        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }

        public int TicketId { get; set; }
        public string UserId { get; set; }

        //Navigation
        public virtual Ticket Ticket { get; set; }
        public virtual BTUser User { get; set; }
    }
}

namespace clientFTP
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Users
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string name { get; set; }

        [StringLength(50)]
        public string pass { get; set; }
    }
}

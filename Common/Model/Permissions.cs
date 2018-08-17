﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FTN.Common.Model
{
	public class Permission
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[XmlIgnore]
		public int Id { get; set; }
		[XmlAttribute]
		public string Name { get; set; } //DNAAuthority ima List<Permission> sto tabeli Permissions daje vezu ka DNAAuthority
		[NotMapped]
		[XmlAttribute]
		public string Description { get; set; }

		public Permission() { }

		public Permission(string name)
		{
			this.Name = name;
		}

		public Permission(string name, string desc)
		{
			this.Name = name;
			this.Description = desc;
		}
		//public List<string> PermList { get; set;  //vrati se ovamo

		//public Permission(List<string> pl)
		//{
		//	this.PermList = pl;
		//}
	}
}
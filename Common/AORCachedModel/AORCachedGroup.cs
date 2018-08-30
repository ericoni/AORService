﻿using FTN.Services.NetworkModelService.DataModel.Wires;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Common.AORCachedModel
{
	[Serializable]
	public class AORCachedGroup : AORCachedEntity
	{
		public List<SynchronousMachine> SMachines { get; set; }
		public List<AORCachedArea> Areas { get; set; }

		public AORCachedGroup()
		{
			SMachines = new List<SynchronousMachine>();
			Areas = new List<AORCachedArea>();
		}
		public AORCachedGroup(string name, List<SynchronousMachine> sms) : base(name) // vrati se da obrises ovaj
		{
			this.SMachines = sms;
		}

		public AORCachedGroup(string name, List<SynchronousMachine> sms, List<AORCachedArea> areas) : base(name)
		{
			this.SMachines = sms;
			this.Areas = areas;
		}
	}
}
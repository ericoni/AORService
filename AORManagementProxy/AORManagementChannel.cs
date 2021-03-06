﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using AORCommon.AORContract;
using System.Security;
using FTN.Common.AORContract;
using FTN.Common.Model;

namespace AORManagement
{
	public class AORManagementChannel : ClientBase<IAORManagement>, IAORManagement
	{
		public AORManagementChannel()
			: base("AORViewerComm") // vrati se da sredis ovo
		{
		}

		public bool Login(string username, string password)
		{
			return this.Channel.Login(username, password);
		}
	}
}

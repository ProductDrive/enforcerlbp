using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace enforcerWeb.Controllers
{
    [Route("api/[controller]")]    //api/Accounts
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IMediator _mediatR;

        public AccountController(IMediator mediatR)
        {
            _mediatR = mediatR;
        }
    }
}

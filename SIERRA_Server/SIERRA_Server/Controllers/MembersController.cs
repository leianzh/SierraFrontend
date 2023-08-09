﻿using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIERRA_Server.Models.DTOs.Members;
using SIERRA_Server.Models.EFModels;
using SIERRA_Server.Models.Interfaces;
using SIERRA_Server.Models.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SIERRA_Server.Models.Repository.EFRepository;
using SIERRA_Server.Models.Infra;
using Microsoft.AspNetCore.Authorization;
using System.Xml.Serialization;

namespace SIERRA_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly MemberEFRepository _repo;
        private readonly IConfiguration _config;
        private readonly HashUtility _hashUtility;
        private readonly EmailHelper _emailHelper;

        public MembersController(AppDbContext context, MemberEFRepository repo, IConfiguration config, HashUtility hashUtility, EmailHelper emailHelper)
        {
            _context = context;
            _repo = repo;
            _config = config;
            _hashUtility = hashUtility;
            _emailHelper = emailHelper;

		}

        [HttpPost("Login")]
        [AllowAnonymous]
        public IActionResult Login(LoginDTO request)
        {
            var service = new MemberService(_repo, _hashUtility, _config);

            // 驗證帳密
            var result = service.ValidLogin(request);

            // 驗證失敗
            if (result.IsFail)
            {
                return BadRequest(result.ErrorMessage);
            }

            var token = service.CreateJwtToken(request.Username);
            return Ok(token);
        }

        [HttpPost("Register")]
		[AllowAnonymous]
		public IActionResult Register(RegisterDTO request)
        {
            var service = new MemberService(_repo, _hashUtility,_emailHelper);
			var result = service.Register(request);

			if (result.IsFail)
			{
				return BadRequest(result.ErrorMessage);
			}

			return Ok("註冊完成,請至信箱收取驗證信");
		}

        [HttpGet("ActiveRegister")]
        [AllowAnonymous]
        public IActionResult ActiveRegister([FromQuery]ActiveRegisterDTO request)
        {
            var service = new MemberService(_repo);
            var result = service.ActiveRegister(request);

            if (result.IsFail)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok("驗證成功");
        }

		// GET: api/Members
		[HttpGet]
        public async Task<ActionResult<IEnumerable<Member>>> GetMembers()
        {
            if (_context.Members == null)
            {
                return NotFound();
            }
            return await _context.Members.ToListAsync();
        }

        // GET: api/Members/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Member>> GetMember(int id)
        {
            if (_context.Members == null)
            {
                return NotFound();
            }
            var member = await _context.Members.FindAsync(id);

            if (member == null)
            {
                return NotFound();
            }

            return member;
        }

        // PUT: api/Members/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMember(int id, Member member)
        {
            if (id != member.Id)
            {
                return BadRequest();
            }

            _context.Entry(member).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemberExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Members
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Member>> PostMember(Member member)
        {
            if (_context.Members == null)
            {
                return Problem("Entity set 'AppDbContext.Members'  is null.");
            }
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMember", new { id = member.Id }, member);
        }

        // DELETE: api/Members/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(int id)
        {
            if (_context.Members == null)
            {
                return NotFound();
            }
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MemberExists(int id)
        {
            return (_context.Members?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

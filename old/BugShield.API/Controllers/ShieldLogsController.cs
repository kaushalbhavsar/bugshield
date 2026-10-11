using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using BugShield.API.Models;

namespace BugShield.API.Controllers
{
    public class ShieldLogsController : ApiController
    {
        private BugSkanDBEntities db = new BugSkanDBEntities();

        // GET: api/ShieldLogs
        public IQueryable<ShieldLog> GetShieldLogs()
        {
            return db.ShieldLogs;
        }

        public List<ShieldLog> GetShieldLogsbyDate(string date)
        {

            DateTime SearchDate = DateTime.Parse(date);
            DateTime startDate = SearchDate.Date;

            DateTime endDate = SearchDate.Date.AddDays(1);

            var products = db.ShieldLogs.Where(s => s.Date >= startDate && s.Date < endDate).OrderByDescending(x => x.Url);

            return products.ToList<ShieldLog>();
        }



        public List<ShieldLog> GetShieldLogsbyDomain(string domain)
        {
            var products = db.ShieldLogs.Where(s => s.Url == domain).OrderByDescending(x => x.Url);

            return products.ToList<ShieldLog>();
        }



        // GET: api/ShieldLogs/5
        [ResponseType(typeof(ShieldLog))]
        public async Task<IHttpActionResult> GetShieldLog(string id)
        {
            ShieldLog shieldLog = await db.ShieldLogs.FindAsync(id);
            if (shieldLog == null)
            {
                return NotFound();
            }

            return Ok(shieldLog);
        }

        // PUT: api/ShieldLogs/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutShieldLog(string id, ShieldLog shieldLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != shieldLog.id)
            {
                return BadRequest();
            }

            db.Entry(shieldLog).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShieldLogExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/ShieldLogs
        [ResponseType(typeof(ShieldLog))]
        public async Task<IHttpActionResult> PostShieldLog(ShieldLog shieldLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ShieldLogs.Add(shieldLog);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ShieldLogExists(shieldLog.id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = shieldLog.id }, shieldLog);
        }

        // DELETE: api/ShieldLogs/5
        [ResponseType(typeof(ShieldLog))]
        public async Task<IHttpActionResult> DeleteShieldLog(string id)
        {
            ShieldLog shieldLog = await db.ShieldLogs.FindAsync(id);
            if (shieldLog == null)
            {
                return NotFound();
            }

            db.ShieldLogs.Remove(shieldLog);
            await db.SaveChangesAsync();

            return Ok(shieldLog);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ShieldLogExists(string id)
        {
            return db.ShieldLogs.Count(e => e.id == id) > 0;
        }
    }
}
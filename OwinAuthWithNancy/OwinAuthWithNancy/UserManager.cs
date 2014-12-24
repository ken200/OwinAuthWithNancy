using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System.Security.Claims;

namespace OwinAuthWithNancy
{
    public class MyUser : IUser
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public MyUser()
        {
            Id = Guid.NewGuid().ToString();
        }
    }

    public class MyUserStore : IUserStore<MyUser>
    {
        public MyUserStore(){}

        public Task CreateAsync(MyUser user)
        {
            return Task.Delay(0);
        }

        public Task DeleteAsync(MyUser user)
        {
            return Task.Delay(0);
        }

        public Task<MyUser> FindByIdAsync(string userId)
        {
            throw new NotSupportedException("UserId is not supported");
        }

        private bool ExistUser(string userName)
        {
            if (userName.ToLower() == "admin" || userName.ToLower() == "root")
                return false;
            return true;
        }

        public Task<MyUser> FindByNameAsync(string userName)
        {
            return Task.Run<MyUser>(() =>
            {
                return !ExistUser(userName) ? new MyUser() { UserName = userName } : null;
            });
        }

        public Task UpdateAsync(MyUser user)
        {
            return Task.Delay(0);
        }

        public void Dispose()
        {

        }
    }

    public class MyUserManager
    {
        private UserManager<MyUser> _uMng;

        public MyUserManager(UserManager<MyUser> uMng)
        {
            this._uMng = uMng;
        }

        public MyUserManager()
            : this(new UserManager<MyUser>(new MyUserStore())) { }


        public async Task<ClaimsIdentity> CreateAsync(string username)
        {
            var user = new MyUser() { UserName = username };
            var activeUser = await _uMng.FindByNameAsync(user.UserName);

            if (activeUser == null)
            {
                return await _uMng.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            }
            else
            {
                return null;
            }
        }
    }
}
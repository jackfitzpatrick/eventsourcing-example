using UsersApi.Commands;
using UsersApi.Domain;
using UsersApi.Events;

namespace UsersApi.Services
{
    public static class UserCommandService
    {
        public static UserRegistered Handle(RegisterUser command)
        {
            var (id, name, email) = command;

            return new UserRegistered(id, name, email);
        }

        public static UserNameChanged Handle(User currentUser, ChangeUserName command)
        {
            var (id, newName) = command;

            UserNameChanged evt = currentUser.ChangeUserName(newName);

            return evt;
        }

        public static UserAddressAmended Handle(User currentUser, UpdateAddress command) {
            var (id, address) = command;

            UserAddressAmended evt = currentUser.ChangeUserAddress(address);

            return evt;
        }
    }
}

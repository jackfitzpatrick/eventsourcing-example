using UsersApi.Events;

namespace UsersApi.Domain
{
    public record User
    {
        public Guid Id;
        public string Name;
        public bool Admin;
        public string Email;

        public string? Address;

        private User(string name, string email, Guid id)
        {
            Name = name;
            Email = email;
            Id = id;
        }

        public static User Create(UserRegistered userRegistered) => new User(userRegistered.Name, userRegistered.Email, userRegistered.Id);

        public User Apply(UserNameChanged userNameChanged)
        {
            Console.WriteLine(this);
            Name = userNameChanged.NewName;

            return this;
        }

        public User Apply(UserAddressAmended userAddressAmended)
        {
            Console.WriteLine(this);
            Address = userAddressAmended.Address;

            return this;
        }

        public UserNameChanged ChangeUserName(string newName)
        {
            if (Name == newName)
            {
                throw new InvalidOperationException("User can't update to the same name");
            }

            return new UserNameChanged(Id, newName);
        }

        public UserAddressAmended ChangeUserAddress(string address)
        {
            if (Address == address)
            {
                throw new InvalidOperationException("User can't update to the same address");
            }

            return new UserAddressAmended(Id, address);
        }
    }
}

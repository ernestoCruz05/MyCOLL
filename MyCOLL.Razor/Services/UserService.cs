namespace MyCOLL.UIComponents.Services
{
    public class UserService
    {
        private UserInfo? _currentUser;
        public event Action? OnUserChanged;

        public bool IsLoggedIn => _currentUser != null;
        public UserInfo? CurrentUser => _currentUser;

        public void SetUser(string userId, string name, string email, string role, string? token = null)
        {
            _currentUser = new UserInfo
            {
                Id = userId,
                Name = name,
                Email = email,
                Role = role,
                Token = token
            };
            NotifyUserChanged();
        }

        public void Logout()
        {
            _currentUser = null;
            NotifyUserChanged();
        }

        public string? GetToken() => _currentUser?.Token;

        private void NotifyUserChanged() => OnUserChanged?.Invoke();
    }

    public class UserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Token { get; set; }

        public string Initials
        {
            get
            {
                if (string.IsNullOrEmpty(Name)) return "?";
                var parts = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                    return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
                return parts[0][0].ToString().ToUpper();
            }
        }
    }
}
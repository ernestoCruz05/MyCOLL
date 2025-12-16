namespace MyCOLL.UIComponents.Services
{
    /// <summary>
    /// Service to manage user authentication state
    /// </summary>
    public class UserService
    {
        private UserInfo? _currentUser;

        /// <summary>
        /// Event triggered when user state changes
        /// </summary>
        public event Action? OnUserChanged;

        /// <summary>
        /// Check if user is logged in
        /// </summary>
        public bool IsLoggedIn => _currentUser != null;

        /// <summary>
        /// Get current user info
        /// </summary>
        public UserInfo? CurrentUser => _currentUser;

        /// <summary>
        /// Set logged in user
        /// </summary>
        public void SetUser(string userId, string name, string email, string? token = null)
        {
            _currentUser = new UserInfo
            {
                Id = userId,
                Name = name,
                Email = email,
                Token = token
            };
            NotifyUserChanged();
        }

        /// <summary>
        /// Log out the current user
        /// </summary>
        public void Logout()
        {
            _currentUser = null;
            NotifyUserChanged();
        }

        /// <summary>
        /// Get authentication token
        /// </summary>
        public string? GetToken()
        {
            return _currentUser?.Token;
        }

        /// <summary>
        /// Update user token
        /// </summary>
        public void UpdateToken(string token)
        {
            if (_currentUser != null)
            {
                _currentUser.Token = token;
            }
        }

        private void NotifyUserChanged()
        {
            OnUserChanged?.Invoke();
        }
    }

    /// <summary>
    /// User information model
    /// </summary>
    public class UserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Token { get; set; }

        /// <summary>
        /// Get user initials for avatar display
        /// </summary>
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

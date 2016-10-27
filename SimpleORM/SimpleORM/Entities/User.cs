namespace SimpleORM.Entities
{
    using System;
    using System.Text;
    using SimpleORM.Attributes;

    [Entity(TableName = "Users")]
    public class User
    {
        [Id]
        private int id;
        [Column(ColumnName = "Username")]
        private string username;
        [Column(ColumnName = "Password")]
        private string password;
        [Column(ColumnName = "Age")]
        private int age;
        [Column(ColumnName = "RegistrationDate")]
        private DateTime registrationDate;
        [Column(ColumnName = "LastLoginTime")]
        private DateTime lastLoginTime;
        [Column(ColumnName = "IsActive")]
        private bool isActive;

        public User(string username, 
            string password, 
            int age, 
            DateTime registrationDate,
            DateTime lastLoginTime,
            bool isActive)
        {
            this.Username = username;
            this.Password = password;
            this.Age = age;
            this.RegistrationDate = registrationDate;
            this.LastLoginTime = lastLoginTime;
            this.IsActive = isActive;
        }

        public int Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        public string Username
        {
            get { return this.username; }
            set { this.username = value; }
        }

        public string Password
        {
            get { return this.password; }
            set { this.password = value; }
        }

        public int Age
        {
            get { return this.age; }
            set { this.age = value; }
        }

        public DateTime RegistrationDate
        {
            get { return this.registrationDate; }
            set { this.registrationDate = value; }
        }

        public DateTime LastLoginTime
        {
            get { return this.lastLoginTime; }
            set { this.lastLoginTime = value; }
        }

        public bool IsActive
        {
            get { return this.isActive; }
            set { this.isActive = value; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Username: {this.Username}");
            sb.AppendLine($"    Password: {this.Password}");
            sb.AppendLine($"    Age: {this.Age}");
            sb.AppendLine($"    Registration Date: {this.RegistrationDate}");

            return sb.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Models;

namespace WpfApp1.Services
{
    internal class TimeHelper
    {
        /// <summary>
        /// Определяет время суток по текущему времени
        /// </summary>
        public static string GetTimeOfDayGreeting()
        {
            var currentTime = DateTime.Now.TimeOfDay;

            if (currentTime >= TimeSpan.Parse("10:00") && currentTime <= TimeSpan.Parse("12:00"))
                return "Доброе утро";
            else if (currentTime >= TimeSpan.Parse("12:01") && currentTime <= TimeSpan.Parse("17:00"))
                return "Добрый день";
            else if (currentTime >= TimeSpan.Parse("17:01") && currentTime <= TimeSpan.Parse("19:00"))
                return "Добрый вечер";
            else
                return "Добрый день"; // по умолчанию
        }

        /// <summary>
        /// Проверяет, находится ли текущее время в рабочем интервале
        /// </summary>
        public static bool IsWithinWorkingHours()
        {
            var currentTime = DateTime.Now.TimeOfDay;
            var startTime = TimeSpan.Parse("10:00");
            var endTime = TimeSpan.Parse("19:00");

            return currentTime >= startTime && currentTime <= endTime;
        }

        /// <summary>
        /// Полное приветствие с именем и временем суток с учетом гендера
        /// </summary>
        public static string GetFullGreeting(string lastName, string firstName, string middleName = null, Gender gender = null)
        {
            var greeting = GetTimeOfDayGreeting();
            var fullName = GetFullNameWithGender(lastName, firstName, middleName, gender);

            return $"{greeting}, {fullName}!";
        }

        /// <summary>
        /// Формирует полное имя с приставкой Mr/Mrs
        /// </summary>
        public static string GetFullNameWithGender(string lastName, string firstName, string middleName = null, Gender gender = null)
        {
            string genderPrefix = GetGenderPrefix(gender);
            string fullName = GetFullName(lastName, firstName, middleName);

            return $"{genderPrefix} {fullName}".Trim();
        }

        /// <summary>
        /// Формирует полное имя (Фамилия Имя Отчество)
        /// </summary>
        public static string GetFullName(string lastName, string firstName, string middleName = null)
        {
            if (string.IsNullOrWhiteSpace(middleName))
                return $"{lastName} {firstName}".Trim();
            else
                return $"{lastName} {firstName} {middleName}".Trim();
        }

        /// <summary>
        /// Получает приставку Mr/Mrs в зависимости от гендера
        /// </summary>
        public static string GetGenderPrefix(Gender gender)
        {
            if (gender == null)
                return string.Empty;

            switch (gender.code?.ToLower())
            {
                case "m":
                case "male":
                    return "Mr.";
                case "f":
                case "female":
                    return "Mrs.";
                default:
                    return string.Empty;
            }
        }
    }
}

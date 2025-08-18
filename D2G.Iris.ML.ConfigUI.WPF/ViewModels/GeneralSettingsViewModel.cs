using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using D2G.Iris.ML.Core.Models;

namespace D2G.Iris.ML.ConfigUI.WPF.ViewModels
{
    public class GeneralSettingsViewModel : BaseViewModel
    {
        private string _projectName = "New ML Project";
        private string _author = Environment.UserName;
        private string _description = "New Model Configuration";
        private DateTime _createdDate = DateTime.Now;
        private DateTime _lastModified = DateTime.Now;
        private Priority _priority = Priority.Medium;
        private string _notes = "";

        #region Properties

        public string ProjectName
        {
            get => _projectName;
            set
            {
                if (SetProperty(ref _projectName, value))
                {
                    LastModified = DateTime.Now;
                }
            }
        }

        public string Author
        {
            get => _author;
            set
            {
                if (SetProperty(ref _author, value))
                {
                    LastModified = DateTime.Now;
                }
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (SetProperty(ref _description, value))
                {
                    LastModified = DateTime.Now;
                }
            }
        }

        public DateTime CreatedDate
        {
            get => _createdDate;
            set => SetProperty(ref _createdDate, value);
        }

        public DateTime LastModified
        {
            get => _lastModified;
            set => SetProperty(ref _lastModified, value);
        }

        public Priority Priority
        {
            get => _priority;
            set
            {
                if (SetProperty(ref _priority, value))
                {
                    LastModified = DateTime.Now;
                }
            }
        }

        public string Notes
        {
            get => _notes;
            set
            {
                if (SetProperty(ref _notes, value))
                {
                    LastModified = DateTime.Now;
                }
            }
        }

        public IEnumerable<Priority> AvailablePriorities => Enum.GetValues<Priority>();

        #endregion

        public void SetConfiguration(ModelConfig config)
        {
            if (config == null) return;

            // Basic info
            Author = config.Author ?? Environment.UserName;
            Description = config.Description ?? "New Model Configuration";

            // Extended properties (if they exist in ModelConfig)
            if (config.ExtendedProperties != null)
            {
                if (config.ExtendedProperties.TryGetValue("ProjectName", out var projectName))
                    ProjectName = projectName?.ToString() ?? "New ML Project";

                if (config.ExtendedProperties.TryGetValue("CreatedDate", out var createdDate) && DateTime.TryParse(createdDate?.ToString(), out var created))
                    CreatedDate = created;

                if (config.ExtendedProperties.TryGetValue("LastModified", out var lastModified) && DateTime.TryParse(lastModified?.ToString(), out var modified))
                    LastModified = modified;

                if (config.ExtendedProperties.TryGetValue("Priority", out var priority) && Enum.TryParse<Priority>(priority?.ToString(), out var parsedPriority))
                    Priority = parsedPriority;

                if (config.ExtendedProperties.TryGetValue("Notes", out var notes))
                    Notes = notes?.ToString() ?? "";
            }
        }

        public void UpdateConfiguration(ModelConfig config)
        {
            if (config == null) return;

            // Basic properties
            config.Author = Author;
            config.Description = Description;

            // Extended properties
            config.ExtendedProperties ??= new Dictionary<string, object>();

            config.ExtendedProperties["ProjectName"] = ProjectName;
            config.ExtendedProperties["CreatedDate"] = CreatedDate;
            config.ExtendedProperties["LastModified"] = LastModified;
            config.ExtendedProperties["Priority"] = Priority.ToString();
            config.ExtendedProperties["Notes"] = Notes;
        }
    }

    public enum Priority
    {
        [Description("Low Priority")]
        Low,
        [Description("Medium Priority")]
        Medium,
        [Description("High Priority")]
        High,
        [Description("Critical Priority")]
        Critical
    }
}
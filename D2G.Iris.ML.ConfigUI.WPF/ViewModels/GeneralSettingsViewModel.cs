using System;
using System.Collections.Generic;
using System.Linq;
using D2G.Iris.ML.Core.Enums;
using D2G.Iris.ML.Core.Models;

namespace D2G.Iris.ML.ConfigUI.WPF.ViewModels
{
    public class GeneralSettingsViewModel : BaseViewModel
    {
        private string _author = Environment.UserName;
        private string _description = "New Model Configuration";
        private ModelType _modelType = ModelType.BinaryClassification;
        private string _targetField = "Label";

        public event Action<ModelType>? ModelTypeChanged;

        #region Properties

        public string Author
        {
            get => _author;
            set => SetProperty(ref _author, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public ModelType ModelType
        {
            get => _modelType;
            set
            {
                if (SetProperty(ref _modelType, value))
                {
                    ModelTypeChanged?.Invoke(value);
                }
            }
        }

        public string TargetField
        {
            get => _targetField;
            set => SetProperty(ref _targetField, value);
        }

        public IEnumerable<ModelType> AvailableModelTypes => Enum.GetValues<ModelType>();

        #endregion

        public void SetConfiguration(ModelConfig config)
        {
            if (config == null) return;

            Author = config.Author ?? Environment.UserName;
            Description = config.Description ?? "New Model Configuration";
            ModelType = config.ModelType;
            TargetField = config.TargetField ?? "Label";
        }

        public void UpdateConfiguration(ModelConfig config)
        {
            if (config == null) return;

            config.Author = Author;
            config.Description = Description;
            config.ModelType = ModelType;
            config.TargetField = TargetField;
        }
    }
}
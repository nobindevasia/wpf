using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.ML.Trainers;
using Microsoft.ML.Trainers.FastTree;
using Microsoft.ML.Trainers.LightGbm;
using D2G.Iris.ML.Core.Enums;

namespace D2G.Iris.ML.ConfigUI.WPF.Utilities
{
    public static class AlgorithmRegistry
    {
        private static readonly Dictionary<string, Dictionary<ModelType, Type>> _algorithmTypeMapping =
            new Dictionary<string, Dictionary<ModelType, Type>>(StringComparer.OrdinalIgnoreCase)
            {
                ["lightgbm"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(LightGbmBinaryTrainer.Options),
                    [ModelType.MultiClassClassification] = typeof(LightGbmMulticlassTrainer.Options),
                    [ModelType.Regression] = typeof(LightGbmRegressionTrainer.Options)
                },

                ["fastforest"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(FastForestBinaryTrainer.Options),
                    [ModelType.MultiClassClassification] = typeof(FastForestBinaryTrainer.Options),
                    [ModelType.Regression] = typeof(FastForestRegressionTrainer.Options)
                },

                ["fasttree"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(FastTreeBinaryTrainer.Options),
                    [ModelType.MultiClassClassification] = typeof(FastTreeBinaryTrainer.Options),
                    [ModelType.Regression] = typeof(FastTreeRegressionTrainer.Options)
                },

                ["gam"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(GamBinaryTrainer.Options),
                    [ModelType.Regression] = typeof(GamRegressionTrainer.Options)
                },

                ["sdca"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(SdcaNonCalibratedBinaryTrainer.Options),
                    [ModelType.MultiClassClassification] = typeof(SdcaNonCalibratedMulticlassTrainer.Options),
                    [ModelType.Regression] = typeof(SdcaRegressionTrainer.Options)
                },

                ["sdcalogisticregression"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(SdcaLogisticRegressionBinaryTrainer.Options)
                },

                ["averagedperceptron"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(AveragedPerceptronTrainer.Options)
                },

                ["linearsvm"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(LinearSvmTrainer.Options)
                },

                ["ldsvm"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(LdSvmTrainer.Options)
                },

                ["sgdcalibrated"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(SgdCalibratedTrainer.Options)
                },

                ["symbolicsgdlogisticregression"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(SymbolicSgdLogisticRegressionBinaryTrainer.Options)
                },

                ["fieldawarefactorizationmachine"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(FieldAwareFactorizationMachineTrainer.Options)
                },

                ["lbfgslogisticregression"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.BinaryClassification] = typeof(LbfgsLogisticRegressionBinaryTrainer.Options)
                },

                ["ols"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.Regression] = typeof(OlsTrainer.Options)
                },

                ["onlinegradientdescent"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.Regression] = typeof(OnlineGradientDescentTrainer.Options)
                },

                ["fasttreetweedie"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.Regression] = typeof(FastTreeTweedieTrainer.Options)
                },

                ["lbfgspoissonregression"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.Regression] = typeof(LbfgsPoissonRegressionTrainer.Options)
                },

                ["sdcamaximumentropy"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.MultiClassClassification] = typeof(SdcaMaximumEntropyMulticlassTrainer.Options)
                },

                ["lbfgsmaximumentropy"] = new Dictionary<ModelType, Type>
                {
                    [ModelType.MultiClassClassification] = typeof(LbfgsMaximumEntropyMulticlassTrainer.Options)
                }
            };

        private static readonly Dictionary<ModelType, List<string>> _algorithmsByModelType =
            new Dictionary<ModelType, List<string>>
            {
                [ModelType.BinaryClassification] = new List<string>
                {
                    "FastForest",
                    "FastTree",
                    "LightGbm",
                    "SdcaLogisticRegression",
                    "Gam",
                    "AveragedPerceptron",
                    "LinearSvm",
                    "LdSvm",
                    "Sdca",
                    "SgdCalibrated",
                    "SymbolicSgdLogisticRegression",
                    "FieldAwareFactorizationMachine",
                    "LbfgsLogisticRegression"
                },
                [ModelType.MultiClassClassification] = new List<string>
                {
                    "LightGbm",
                    "SdcaMaximumEntropy",
                    "Sdca",
                    "FastTree",
                    "FastForest",
                    "LbfgsMaximumEntropy"
                },
                [ModelType.Regression] = new List<string>
                {
                    "FastForest",
                    "FastTree",
                    "LightGbm",
                    "Ols",
                    "OnlineGradientDescent",
                    "Gam",
                    "Sdca",
                    "FastTreeTweedie",
                    "LbfgsPoissonRegression"
                }
            };

        public static Type? GetOptionsType(string algorithmName, ModelType modelType)
        {
            string algorithmLower = algorithmName?.ToLower();

            if (string.IsNullOrEmpty(algorithmLower))
                return null;

            if (_algorithmTypeMapping.TryGetValue(algorithmLower, out var modelTypeMap))
            {
                if (modelTypeMap.TryGetValue(modelType, out var optionsType))
                {
                    return optionsType;
                }
            }

            return null;
        }

        public static List<string> GetAlgorithmsForModelType(ModelType modelType)
        {
            return _algorithmsByModelType.TryGetValue(modelType, out var algorithms)
                ? new List<string>(algorithms)
                : new List<string>();
        }
    }

    public static class ParameterHelper
    {
        private static readonly HashSet<string> ExcludedParameterNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "LabelColumnName",
            "FeatureColumnName",
            "ExampleWeightColumnName",
            "RowGroupColumnName",
            "GroupIdColumnName",
            "ScoreColumnName",
            "PredictedLabelColumnName",
            "ProbabilityColumnName"
        };

        public static bool IsConfigurableParameter(string name, Type type)
        {
            if (ExcludedParameterNames.Contains(name))
                return false;

            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return underlyingType.IsPrimitive ||
                   underlyingType == typeof(string) ||
                   underlyingType == typeof(decimal) ||
                   underlyingType.IsEnum ||
                   underlyingType == typeof(TimeSpan);
        }

        public static List<PropertyInfo> GetConfigurableProperties(Type optionsType)
        {
            if (optionsType == null)
                return new List<PropertyInfo>();

            return optionsType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.CanRead)
                .Where(p => IsConfigurableParameter(p.Name, p.PropertyType))
                .OrderBy(p => p.Name)
                .ToList();
        }

        public static List<FieldInfo> GetConfigurableFields(Type optionsType)
        {
            if (optionsType == null)
                return new List<FieldInfo>();

            return optionsType.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => !f.IsInitOnly && !f.IsLiteral)
                .Where(f => IsConfigurableParameter(f.Name, f.FieldType))
                .OrderBy(f => f.Name)
                .ToList();
        }
    }
}
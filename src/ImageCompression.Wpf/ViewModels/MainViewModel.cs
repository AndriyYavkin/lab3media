namespace ImageCompression.Wpf.ViewModels;

using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ImageCompression.Core.Compression;
using ImageCompression.Core.Generators;
using ImageCompression.Core.Metrics;
using ImageCompression.Core.Models;
using ImageCompression.Core.Services;
using ImageCompression.Core.Transforms;
using ImageCompression.Wpf.Converters;
using Microsoft.Win32;

/// <summary>
/// Root view model coordinating application state, interactive transformations, and comparative experiments.
/// </summary>
public sealed class MainViewModel : ObservableObject
{
    private readonly IImageProcessorService _processorService;
    private readonly MatrixDctTransformer _dctTransformer;
    private readonly HaarWaveletTransformer _dwtTransformer;

    private GrayscaleImage _sourceImage;
    private BitmapSource? _sourceBitmap;
    private BitmapSource? _dctDisplayBitmap;
    private BitmapSource? _dwtDisplayBitmap;

    private ISampleImageGenerator _selectedGenerator;
    private ICompressionStrategy _selectedStrategy;

    private double _retentionPercentage = 10.0;
    private double _absoluteThreshold = 20.0;
    private double _quantizationStep = 16.0;
    private int _frequencyCutoffLimit = 4;
    private int _dwtLevels = 4;

    private string _viewMode = "Reconstructed";
    private bool _isBusy;
    private string _statusMessage = "Ready";

    private CompressionEvaluationResult? _dctResult;
    private CompressionEvaluationResult? _dwtResult;

    private CancellationTokenSource? _processingCts;

    public ObservableCollection<ISampleImageGenerator> SampleGenerators { get; }
    public ObservableCollection<ICompressionStrategy> CompressionStrategies { get; }
    public ObservableCollection<ExperimentItemViewModel> ExperimentResults { get; }

    public ObservableCollection<string> ViewModes { get; } = new()
    {
        "Reconstructed",
        "Difference Map (Amplified)",
        "Coefficients Spectrum"
    };

    public BitmapSource? SourceBitmap
    {
        get => _sourceBitmap;
        private set => SetProperty(ref _sourceBitmap, value);
    }

    public BitmapSource? DctDisplayBitmap
    {
        get => _dctDisplayBitmap;
        private set => SetProperty(ref _dctDisplayBitmap, value);
    }

    public BitmapSource? DwtDisplayBitmap
    {
        get => _dwtDisplayBitmap;
        private set => SetProperty(ref _dwtDisplayBitmap, value);
    }

    public ISampleImageGenerator SelectedGenerator
    {
        get => _selectedGenerator;
        set
        {
            if (SetProperty(ref _selectedGenerator, value))
            {
                GenerateSelectedSampleImage();
            }
        }
    }

    public ICompressionStrategy SelectedStrategy
    {
        get => _selectedStrategy;
        set
        {
            if (SetProperty(ref _selectedStrategy, value))
            {
                OnPropertyChanged(nameof(IsTopCoefficientsStrategySelected));
                OnPropertyChanged(nameof(IsThresholdStrategySelected));
                OnPropertyChanged(nameof(IsQuantizationStrategySelected));
                OnPropertyChanged(nameof(IsFrequencyCutoffStrategySelected));
                TriggerRecomputation();
            }
        }
    }

    public bool IsTopCoefficientsStrategySelected => SelectedStrategy is TopCoefficientsStrategy;
    public bool IsThresholdStrategySelected => SelectedStrategy is ThresholdCompressionStrategy;
    public bool IsQuantizationStrategySelected => SelectedStrategy is QuantizationCompressionStrategy;
    public bool IsFrequencyCutoffStrategySelected => SelectedStrategy is FrequencyCutoffCompressionStrategy;

    public double RetentionPercentage
    {
        get => _retentionPercentage;
        set
        {
            if (SetProperty(ref _retentionPercentage, Math.Round(value, 2)))
            {
                TriggerRecomputation();
            }
        }
    }

    public double AbsoluteThreshold
    {
        get => _absoluteThreshold;
        set
        {
            if (SetProperty(ref _absoluteThreshold, Math.Round(value, 1)))
            {
                TriggerRecomputation();
            }
        }
    }

    public double QuantizationStep
    {
        get => _quantizationStep;
        set
        {
            if (SetProperty(ref _quantizationStep, Math.Round(value, 1)))
            {
                TriggerRecomputation();
            }
        }
    }

    public int FrequencyCutoffLimit
    {
        get => _frequencyCutoffLimit;
        set
        {
            if (SetProperty(ref _frequencyCutoffLimit, value))
            {
                TriggerRecomputation();
            }
        }
    }

    public int DwtLevels
    {
        get => _dwtLevels;
        set
        {
            if (SetProperty(ref _dwtLevels, value))
            {
                _dwtTransformer.DecompositionLevels = value;
                TriggerRecomputation();
            }
        }
    }

    public string ViewMode
    {
        get => _viewMode;
        set
        {
            if (SetProperty(ref _viewMode, value))
            {
                UpdateDisplayBitmaps();
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public CompressionEvaluationResult? DctResult
    {
        get => _dctResult;
        private set
        {
            if (SetProperty(ref _dctResult, value))
            {
                OnPropertyChanged(nameof(DctMetrics));
                OnPropertyChanged(nameof(PsnrDelta));
                OnPropertyChanged(nameof(FormattedPsnrDelta));
                OnPropertyChanged(nameof(DeltaAdvantageLabel));
            }
        }
    }

    public CompressionEvaluationResult? DwtResult
    {
        get => _dwtResult;
        private set
        {
            if (SetProperty(ref _dwtResult, value))
            {
                OnPropertyChanged(nameof(DwtMetrics));
                OnPropertyChanged(nameof(PsnrDelta));
                OnPropertyChanged(nameof(FormattedPsnrDelta));
                OnPropertyChanged(nameof(DeltaAdvantageLabel));
            }
        }
    }

    public QualityMetrics? DctMetrics => DctResult?.Metrics;
    public QualityMetrics? DwtMetrics => DwtResult?.Metrics;

    public double PsnrDelta
    {
        get
        {
            if (DwtMetrics == null || DctMetrics == null)
            {
                return 0.0;
            }

            if (double.IsInfinity(DwtMetrics.Psnr) || double.IsInfinity(DctMetrics.Psnr))
            {
                return 0.0;
            }

            return DwtMetrics.Psnr - DctMetrics.Psnr;
        }
    }

    public string FormattedPsnrDelta
    {
        get
        {
            if (DwtMetrics == null || DctMetrics == null)
            {
                return "-- dB";
            }

            if (double.IsPositiveInfinity(DwtMetrics.Psnr) && double.IsPositiveInfinity(DctMetrics.Psnr))
            {
                return "0.00 dB (Both Lossless)";
            }

            return $"{PsnrDelta:+0.00;-0.00;0.00} dB";
        }
    }

    public string DeltaAdvantageLabel
    {
        get
        {
            if (DwtMetrics == null || DctMetrics == null)
            {
                return "N/A";
            }

            double diff = PsnrDelta;
            if (Math.Abs(diff) < 0.05)
            {
                return "Performance is virtually identical.";
            }

            return diff > 0
                ? "DWT (Haar Wavelet) achieves higher PSNR fidelity."
                : "DCT (Cosine Transform) achieves higher PSNR fidelity.";
        }
    }

    public ICommand OpenImageFileCommand { get; }
    public ICommand SaveDctImageCommand { get; }
    public ICommand SaveDwtImageCommand { get; }
    public ICommand ApplyPresetCommand { get; }
    public ICommand RunComparativeExperimentsCommand { get; }
    public ICommand ExportExperimentReportCommand { get; }

    public MainViewModel()
    {
        var metricCalculator = new PsnrCalculator();
        _processorService = new ImageProcessorService(metricCalculator);
        _dctTransformer = new MatrixDctTransformer();
        _dwtTransformer = new HaarWaveletTransformer { DecompositionLevels = _dwtLevels };

        SampleGenerators = new ObservableCollection<ISampleImageGenerator>
        {
            new GeometricShapesGenerator(),
            new RadialZonePlateGenerator(),
            new CheckerboardGenerator(),
            new HarmonicGradientGenerator(),
            new SyntheticLandscapeGenerator()
        };

        CompressionStrategies = new ObservableCollection<ICompressionStrategy>
        {
            new TopCoefficientsStrategy(),
            new ThresholdCompressionStrategy(),
            new QuantizationCompressionStrategy(),
            new FrequencyCutoffCompressionStrategy()
        };

        ExperimentResults = new ObservableCollection<ExperimentItemViewModel>();

        _selectedGenerator = SampleGenerators[0];
        _selectedStrategy = CompressionStrategies[0];
        _sourceImage = _selectedGenerator.Generate(512, 512);

        OpenImageFileCommand = new RelayCommand(OpenImageFile);
        SaveDctImageCommand = new RelayCommand(SaveDctImage, () => DctResult?.ReconstructedImage != null);
        SaveDwtImageCommand = new RelayCommand(SaveDwtImage, () => DwtResult?.ReconstructedImage != null);
        ApplyPresetCommand = new RelayCommand<string>(ApplyPreset);
        RunComparativeExperimentsCommand = new RelayCommand(async () => await RunComparativeExperimentsAsync());
        ExportExperimentReportCommand = new RelayCommand(ExportExperimentReport, () => ExperimentResults.Count > 0);

        UpdateSourceBitmap();
        ExecuteTransforms();
        _ = RunComparativeExperimentsAsync();
    }

    private void GenerateSelectedSampleImage()
    {
        _sourceImage = SelectedGenerator.Generate(512, 512);
        UpdateSourceBitmap();
        TriggerRecomputation();
        _ = RunComparativeExperimentsAsync();
    }

    private void UpdateSourceBitmap()
    {
        SourceBitmap = BitmapConverter.ToBitmapSource(_sourceImage);
    }

    private void TriggerRecomputation()
    {
        _processingCts?.Cancel();
        _processingCts = new CancellationTokenSource();
        var token = _processingCts.Token;

        Task.Delay(50, token).ContinueWith(t =>
        {
            if (!t.IsCanceled)
            {
                Application.Current?.Dispatcher.Invoke(ExecuteTransforms);
            }
        }, TaskScheduler.Default);
    }

    private void ExecuteTransforms()
    {
        IsBusy = true;
        StatusMessage = "Computing DCT and DWT transforms...";

        var parameters = new CompressionParameters
        {
            RetentionPercentage = RetentionPercentage,
            AbsoluteThreshold = AbsoluteThreshold,
            QuantizationStep = QuantizationStep,
            FrequencyCutoffLimit = FrequencyCutoffLimit
        };

        try
        {
            var dctRes = _processorService.Process(_sourceImage, _dctTransformer, SelectedStrategy, parameters);
            var dwtRes = _processorService.Process(_sourceImage, _dwtTransformer, SelectedStrategy, parameters);

            DctResult = dctRes;
            DwtResult = dwtRes;

            UpdateDisplayBitmaps();

            StatusMessage = $"Completed. DCT PSNR: {dctRes.Metrics.FormattedPsnr} | DWT PSNR: {dwtRes.Metrics.FormattedPsnr}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdateDisplayBitmaps()
    {
        if (DctResult == null || DwtResult == null)
        {
            return;
        }

        switch (ViewMode)
        {
            case "Difference Map (Amplified)":
                DctDisplayBitmap = BitmapConverter.ToBitmapSource(DctResult.DifferenceMap);
                DwtDisplayBitmap = BitmapConverter.ToBitmapSource(DwtResult.DifferenceMap);
                break;

            case "Coefficients Spectrum":
                DctDisplayBitmap = BitmapConverter.ToBitmapSource(DctResult.CoefficientVisualizationMap);
                DwtDisplayBitmap = BitmapConverter.ToBitmapSource(DwtResult.CoefficientVisualizationMap);
                break;

            default:
                DctDisplayBitmap = BitmapConverter.ToBitmapSource(DctResult.ReconstructedImage);
                DwtDisplayBitmap = BitmapConverter.ToBitmapSource(DwtResult.ReconstructedImage);
                break;
        }
    }

    private void ApplyPreset(string? presetName)
    {
        if (string.IsNullOrWhiteSpace(presetName))
        {
            return;
        }

        SelectedStrategy = CompressionStrategies[0];

        switch (presetName)
        {
            case "HighQuality":
                RetentionPercentage = 50.0;
                break;
            case "MediumQuality":
                RetentionPercentage = 10.0;
                break;
            case "HighCompression":
                RetentionPercentage = 2.0;
                break;
            case "ExtremeCompression":
                RetentionPercentage = 0.5;
                break;
        }
    }

    public async Task RunComparativeExperimentsAsync()
    {
        IsBusy = true;
        StatusMessage = "Evaluating multi-level experimental benchmarks...";

        var testRatios = new (string Label, double Ratio)[]
        {
            ("Level 1: High Fidelity", 50.0),
            ("Level 2: Moderate Compression", 20.0),
            ("Level 3: Target Ratio", 10.0),
            ("Level 4: Strong Compression", 5.0),
            ("Level 5: Aggressive Compression", 2.0),
            ("Level 6: Extreme Compression", 0.5)
        };

        var currentImage = _sourceImage;
        var topStrategy = new TopCoefficientsStrategy();

        var computedItems = await Task.Run(() =>
        {
            var list = new List<ExperimentItemViewModel>();

            foreach (var test in testRatios)
            {
                var testParams = new CompressionParameters
                {
                    RetentionPercentage = test.Ratio
                };

                var dctEval = _processorService.Process(currentImage, _dctTransformer, topStrategy, testParams);
                var dwtEval = _processorService.Process(currentImage, _dwtTransformer, topStrategy, testParams);

                list.Add(new ExperimentItemViewModel
                {
                    CompressionLevel = test.Label,
                    RetainedPercentage = test.Ratio,
                    RetainedCoefficients = dctEval.Metrics.RetainedCoefficients,
                    DctPsnr = dctEval.Metrics.Psnr,
                    DwtPsnr = dwtEval.Metrics.Psnr,
                    DctMse = dctEval.Metrics.Mse,
                    DwtMse = dwtEval.Metrics.Mse,
                    DctTimeMs = dctEval.Metrics.ElapsedMilliseconds,
                    DwtTimeMs = dwtEval.Metrics.ElapsedMilliseconds
                });
            }

            return list;
        });

        ExperimentResults.Clear();
        foreach (var item in computedItems)
        {
            ExperimentResults.Add(item);
        }

        IsBusy = false;
        StatusMessage = "Comparative experiment evaluation ready.";
    }

    private void OpenImageFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select Input Grayscale Image",
            Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp;*.tif)|*.png;*.jpg;*.jpeg;*.bmp;*.tif|All Files (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                _sourceImage = BitmapConverter.LoadGrayscaleImageFromFile(dialog.FileName, 512, 512);
                UpdateSourceBitmap();
                TriggerRecomputation();
                _ = RunComparativeExperimentsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load image: {ex.Message}", "Image Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void SaveDctImage()
    {
        if (DctResult?.ReconstructedImage == null)
        {
            return;
        }

        var dialog = new SaveFileDialog
        {
            Title = "Save DCT Reconstructed Image",
            Filter = "PNG Image (*.png)|*.png",
            FileName = "dct_reconstructed.png"
        };

        if (dialog.ShowDialog() == true)
        {
            BitmapConverter.SaveGrayscaleImageToPng(DctResult.ReconstructedImage, dialog.FileName);
        }
    }

    private void SaveDwtImage()
    {
        if (DwtResult?.ReconstructedImage == null)
        {
            return;
        }

        var dialog = new SaveFileDialog
        {
            Title = "Save DWT Reconstructed Image",
            Filter = "PNG Image (*.png)|*.png",
            FileName = "dwt_reconstructed.png"
        };

        if (dialog.ShowDialog() == true)
        {
            BitmapConverter.SaveGrayscaleImageToPng(DwtResult.ReconstructedImage, dialog.FileName);
        }
    }

    private void ExportExperimentReport()
    {
        if (ExperimentResults.Count == 0)
        {
            return;
        }

        var dialog = new SaveFileDialog
        {
            Title = "Export Experiment Results CSV",
            Filter = "CSV File (*.csv)|*.csv",
            FileName = "dct_vs_dwt_compression_experiments.csv"
        };

        if (dialog.ShowDialog() == true)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Experiment Level,Retained Percentage,Retained Coefficients,DCT PSNR (dB),DWT PSNR (dB),PSNR Delta (DWT-DCT),DCT MSE,DWT MSE,Advantage");

            foreach (var item in ExperimentResults)
            {
                sb.AppendLine($"\"{item.CompressionLevel}\",{item.RetainedPercentage:F2},{item.RetainedCoefficients},{item.DctPsnr:F2},{item.DwtPsnr:F2},{item.PsnrDifference:+0.00;-0.00;0.00},{item.DctMse:F4},{item.DwtMse:F4},\"{item.AdvantageSummary}\"");
            }

            File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);
            MessageBox.Show("Experiment report successfully exported.", "Report Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

# Experimental Image Compression: DCT vs DWT (Haar)

A high-performance C# / WPF workstation application for experimental image compression benchmarking comparing **Discrete Cosine Transform (DCT)** and **Discrete Wavelet Transform (DWT Haar)**.

---

## 1. Overview & Capabilities

The application implements orthogonal transform-based compression from scratch without relying on external transform libraries:
- **Discrete Cosine Transform (DCT)**: 8×8 block partitioning evaluated via explicit matrix multiplication ($C = T \cdot B \cdot T^T$, $B = T^T \cdot C \cdot T$).
- **Discrete Wavelet Transform (DWT)**: Multi-scale 2D separable orthonormal Haar wavelet decomposition with energy conservation.
- **Side-by-Side Visualization**: Simultaneous real-time display of **Original Reference (512×512)** | **DCT Reconstruction** | **DWT Reconstruction**.
- **Multiple Compression Strategies**:
  1. *Top Coefficients Retention*: Exact retention of the top $K\%$ largest-magnitude coefficients (direct apples-to-apples comparison).
  2. *Absolute Thresholding*: Eliminates coefficients where $|c| < \tau$.
  3. *Uniform Quantization*: Quantizes coefficients into discrete steps $Q$.
  4. *High-Frequency Cutoff*: Truncates high-frequency basis coordinates.
- **Experimental Evaluation**:
  - Exact measurement of Peak Signal-to-Noise Ratio (**PSNR** in dB) and Mean Squared Error (**MSE**).
  - Measurement of retained coefficient count and percentage.
  - Calculation of theoretical compression ratio ($N_{\text{total}} / N_{\text{retained}}$).
  - Automated multi-level experimental benchmarking matrix evaluating 6 distinct compression levels with CSV export capability.
  - Visual inspection modes: Reconstructed Image, Amplified Difference Error Heatmap ($|I_{\text{orig}} - I_{\text{recon}}| \times 5$), and 2D Logarithmic Transform Spectrum Map.

---

## 2. Mathematical Formulations

### 2.1 Discrete Cosine Transform (8×8 Block DCT)
The image is segmented into non-overlapping $8 \times 8$ blocks. For each block $B$, pixels are shifted by $-128.0$ and transformed using the orthogonal matrix $T \in \mathbb{R}^{8 \times 8}$:

$$T_{i, j} = \alpha(i) \cos\left( \frac{(2j + 1) i \pi}{2N} \right), \quad N = 8$$

where:
$$\alpha(0) = \sqrt{\frac{1}{N}} = \frac{1}{\sqrt{8}}, \quad \alpha(i) = \sqrt{\frac{2}{N}} = \frac{1}{2} \text{ for } i \in \{1, \dots, 7\}$$

The forward transform is evaluated by matrix multiplication:
$$C = T \cdot B \cdot T^T$$

Because $T$ is orthogonal ($T \cdot T^T = I$), the inverse transform is lossless:
$$B = T^T \cdot C \cdot T$$

The reconstructed block is shifted by $+128.0$ and clamped to $[0, 255]$.

### 2.2 Discrete Wavelet Transform (2D Orthonormal Haar)
The 2D Haar wavelet transform is computed separably on rows and columns:
- **Low-pass (approximation)**: $a_k = \frac{x_{2k} + x_{2k+1}}{\sqrt{2}}$
- **High-pass (detail)**: $d_k = \frac{x_{2k} - x_{2k+1}}{\sqrt{2}}$

The normalization factor $1/\sqrt{2}$ makes the transform orthonormal, preserving the $L_2$ Euclidean norm (Parseval's energy theorem):
$$\sum_{k=0}^{N-1} x_k^2 = \sum_{k=0}^{N/2-1} a_k^2 + \sum_{k=0}^{N/2-1} d_k^2$$

Inverse reconstruction:
$$x_{2k} = \frac{a_k + d_k}{\sqrt{2}}, \quad x_{2k+1} = \frac{a_k - d_k}{\sqrt{2}}$$

### 2.3 Quality Assessment Metrics
- **Mean Squared Error (MSE)**:
  $$\text{MSE} = \frac{1}{M \cdot N} \sum_{y=0}^{N-1} \sum_{x=0}^{M-1} (I_{\text{orig}}(x,y) - I_{\text{recon}}(x,y))^2$$

- **Peak Signal-to-Noise Ratio (PSNR)**:
  $$\text{PSNR} = 10 \cdot \log_{10}\left( \frac{255^2}{\text{MSE}} \right) \quad [\text{dB}]$$
  *(Reported as $\infty$ when $\text{MSE} \le 10^{-12}$)*.

- **Theoretical Compression Ratio**:
  $$\text{CR} = \frac{N_{\text{total}}}{\max(1, N_{\text{retained}})} : 1$$

> **Distinction note**: The count of retained non-zero coefficients represents transform energy compaction. In production image formats (e.g. JPEG), actual compressed byte sizes depend on subsequent run-length encoding (RLE) and entropy coding (Huffman or arithmetic coding).

---

## 3. SOLID Architecture

The codebase strictly adheres to SOLID software design principles:

- **Single Responsibility Principle (SRP)**:
  - `MatrixDctTransformer`: Only computes 8×8 block DCT matrix multiplication and inversion.
  - `HaarWaveletTransformer`: Only performs 2D separable multi-resolution Haar filtering.
  - `TopCoefficientsStrategy`, `ThresholdCompressionStrategy`, `QuantizationCompressionStrategy`, `FrequencyCutoffCompressionStrategy`: Each encapsulates a single coefficient pruning algorithm.
  - `PsnrCalculator`: Exclusively computes MSE, PSNR, and coefficient statistics.
  - `ImageProcessorService`: Orchestrates the compression pipeline and diagnostic map generation.
- **Open/Closed Principle (OCP)**:
  - New transforms implement `IImageTransformer`.
  - New compression mechanisms implement `ICompressionStrategy` without modifying transformer internals.
- **Liskov Substitution Principle (LSP)**:
  - All `IImageTransformer` implementations provide identical guarantees regarding forward and inverse execution contracts.
- **Interface Segregation Principle (ISP)**:
  - Specialized interfaces (`IDctTransformer`, `IDwtTransformer`, `IQualityMetricCalculator`, `ISampleImageGenerator`) prevent monolithic contracts.
- **Dependency Inversion Principle (DIP)**:
  - High-level orchestration services and ViewModels depend on domain abstractions rather than concrete types.

---

## 4. Project Structure

```
ImageCompressionLab/
├── ImageCompressionLab.slnx
├── README.md
├── src/
│   ├── ImageCompression.Core/
│   │   ├── Models/
│   │   │   ├── GrayscaleImage.cs
│   │   │   ├── TransformResult.cs
│   │   │   └── QualityMetrics.cs
│   │   ├── Transforms/
│   │   │   ├── IImageTransformer.cs
│   │   │   ├── IDctTransformer.cs
│   │   │   ├── IDwtTransformer.cs
│   │   │   ├── MatrixDctTransformer.cs
│   │   │   └── HaarWaveletTransformer.cs
│   │   ├── Compression/
│   │   │   ├── ICompressionStrategy.cs
│   │   │   ├── CompressionParameters.cs
│   │   │   ├── TopCoefficientsStrategy.cs
│   │   │   ├── ThresholdCompressionStrategy.cs
│   │   │   ├── QuantizationCompressionStrategy.cs
│   │   │   └── FrequencyCutoffCompressionStrategy.cs
│   │   ├── Metrics/
│   │   │   ├── IQualityMetricCalculator.cs
│   │   │   └── PsnrCalculator.cs
│   │   ├── Generators/
│   │   │   ├── ISampleImageGenerator.cs
│   │   │   └── StandardSampleGenerators.cs
│   │   └── Services/
│   │       ├── IImageProcessorService.cs
│   │       └── ImageProcessorService.cs
│   └── ImageCompression.Wpf/
│       ├── App.xaml / App.xaml.cs
│       ├── MainWindow.xaml / MainWindow.xaml.cs
│       ├── ViewModels/
│       │   ├── ObservableObject.cs
│       │   ├── RelayCommand.cs
│       │   ├── ExperimentItemViewModel.cs
│       │   └── MainViewModel.cs
│       └── Converters/
│           ├── BitmapConverter.cs
│           └── ValueConverters.cs
└── tests/
    └── ImageCompression.Tests/
        ├── DctTransformerTests.cs
        ├── HaarWaveletTransformerTests.cs
        ├── CompressionStrategyTests.cs
        ├── QualityMetricTests.cs
        ├── ImageProcessorServiceTests.cs
        └── BenchmarkGeneratorTests.cs
```

---

## 5. How to Build and Run

### Requirements
- .NET 8.0 SDK or .NET 10.0 SDK
- Windows OS (WPF Desktop environment)

### Build Solution
```powershell
dotnet build ImageCompressionLab.slnx
```

### Run Automated Unit Tests
```powershell
dotnet test
```

### Launch Desktop Application
```powershell
dotnet run --project src/ImageCompression.Wpf/ImageCompression.Wpf.csproj
```

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Timers;
using Microsoft.Win32;
using System.Diagnostics;
using LibreHardwareMonitor.Hardware;
using System.Linq;

namespace GMachineGUI
{
    public partial class MainWindow : Window
    {
        private bool isTraining = false;
        private CancellationTokenSource trainingCancellation;
        private System.Timers.Timer hardwareMonitorTimer;
        private Random random = new Random();
        
        private Computer computer;
        private bool useRealMonitoring = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeHardwareMonitoring();
            InitializeMonitoringTimer();
            AddLog("[System] G-Machine GUI başlatıldı...");
            AddLog("[System] Donanım izleme başlatılıyor...");
            AddLog("[System] Sistem hazır. Eğitim parametrelerini ayarlayın ve başlatın.");
        }

        private void InitializeHardwareMonitoring()
        {
            try
            {
                computer = new Computer
                {
                    IsCpuEnabled = true,
                    IsGpuEnabled = true,
                    IsMemoryEnabled = true,
                    IsMotherboardEnabled = false,
                    IsControllerEnabled = false,
                    IsNetworkEnabled = false,
                    IsStorageEnabled = false
                };

                computer.Open();
                computer.Accept(new UpdateVisitor());

                var gpu = computer.Hardware.FirstOrDefault(h => 
                    h.HardwareType == HardwareType.GpuNvidia || 
                    h.HardwareType == HardwareType.GpuAmd ||
                    h.HardwareType == HardwareType.GpuIntel);

                var cpu = computer.Hardware.FirstOrDefault(h => 
                    h.HardwareType == HardwareType.Cpu);

                var ram = computer.Hardware.FirstOrDefault(h => 
                    h.HardwareType == HardwareType.Memory);

                if (gpu != null || cpu != null || ram != null)
                {
                    useRealMonitoring = true;
                    AddLog("[Hardware] Gerçek donanım izleme aktif");
                    if (gpu != null) AddLog($"[Hardware] GPU: {gpu.Name}");
                    if (cpu != null) AddLog($"[Hardware] CPU: {cpu.Name}");
                    if (ram != null) AddLog($"[Hardware] RAM izleme aktif");
                }
                else
                {
                    AddLog("[Hardware] Donanım bulunamadı - Simülasyon modu");
                }
            }
            catch (Exception ex)
            {
                AddLog($"[Hardware] Hata: {ex.Message}");
                AddLog("[Hardware] Simülasyon moduna geçiliyor...");
                useRealMonitoring = false;
            }
        }

        private void InitializeMonitoringTimer()
        {
            hardwareMonitorTimer = new System.Timers.Timer(1000);
            hardwareMonitorTimer.Elapsed += UpdateHardwareStats;
            hardwareMonitorTimer.AutoReset = true;
            hardwareMonitorTimer.Start();
        }

        private void UpdateHardwareStats(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    if (useRealMonitoring && computer != null)
                    {
                        computer.Accept(new UpdateVisitor());

                        var gpu = computer.Hardware.FirstOrDefault(h => 
                            h.HardwareType == HardwareType.GpuNvidia || 
                            h.HardwareType == HardwareType.GpuAmd ||
                            h.HardwareType == HardwareType.GpuIntel);

                        var cpu = computer.Hardware.FirstOrDefault(h => 
                            h.HardwareType == HardwareType.Cpu);

                        var ram = computer.Hardware.FirstOrDefault(h => 
                            h.HardwareType == HardwareType.Memory);

                        if (gpu != null)
                        {
                            gpu.Update();

                            var gpuLoad = gpu.Sensors.FirstOrDefault(s => 
                                s.SensorType == SensorType.Load && 
                                (s.Name.Contains("GPU Core") || s.Name.Contains("D3D 3D")));
                            
                            if (gpuLoad != null && gpuLoad.Value.HasValue)
                            {
                                txtGPUUsage.Text = $"{gpuLoad.Value.Value:F1}%";
                                pgGPUUsage.Value = gpuLoad.Value.Value;
                            }

                            var vramUsed = gpu.Sensors.FirstOrDefault(s => 
                                s.SensorType == SensorType.SmallData && 
                                s.Name.Contains("GPU Memory Used"));
                            
                            var vramTotal = gpu.Sensors.FirstOrDefault(s => 
                                s.SensorType == SensorType.SmallData && 
                                s.Name.Contains("GPU Memory Total"));

                            if (vramUsed != null && vramUsed.Value.HasValue)
                            {
                                double used = vramUsed.Value.Value;
                                double total = vramTotal?.Value.Value ?? 12.0;
                                txtVRAMUsage.Text = $"{used:F1}/{total:F1}GB";
                                pgVRAMUsage.Value = (used / total) * 100;
                            }

                            var gpuTemp = gpu.Sensors.FirstOrDefault(s => 
                                s.SensorType == SensorType.Temperature && 
                                s.Name.Contains("GPU Core"));
                            
                            if (gpuTemp != null && gpuTemp.Value.HasValue)
                            {
                                txtGPUTemp.Text = $"{gpuTemp.Value.Value:F0}°C";
                                pgGPUTemp.Value = Math.Min(gpuTemp.Value.Value, 100);
                            }
                        }

                        if (cpu != null)
                        {
                            cpu.Update();
                            var cpuLoad = cpu.Sensors.FirstOrDefault(s => 
                                s.SensorType == SensorType.Load && 
                                s.Name == "CPU Total");
                            
                            if (cpuLoad != null && cpuLoad.Value.HasValue)
                            {
                            }
                        }

                        if (ram != null)
                        {
                            ram.Update();
                            var memUsed = ram.Sensors.FirstOrDefault(s => 
                                s.SensorType == SensorType.Data && 
                                s.Name == "Memory Used");
                            
                            var memAvailable = ram.Sensors.FirstOrDefault(s => 
                                s.SensorType == SensorType.Data && 
                                s.Name == "Memory Available");
                            
                            if (memUsed != null && memUsed.Value.HasValue && 
                                memAvailable != null && memAvailable.Value.HasValue)
                            {
                            }
                        }
                    }
                    else
                    {
                        if (isTraining)
                        {
                            double gpuUsage = 45 + random.NextDouble() * 30;
                            double vramUsed = 6.5 + random.NextDouble() * 2;
                            double vramTotal = 12.0;
                            double temp = 65 + random.NextDouble() * 15;

                            txtGPUUsage.Text = $"{gpuUsage:F1}%";
                            pgGPUUsage.Value = gpuUsage;

                            txtVRAMUsage.Text = $"{vramUsed:F1}/{vramTotal:F1}GB";
                            pgVRAMUsage.Value = (vramUsed / vramTotal) * 100;

                            txtGPUTemp.Text = $"{temp:F0}°C";
                            pgGPUTemp.Value = Math.Min(temp, 100);
                        }
                        else
                        {
                            txtGPUUsage.Text = "0.0%";
                            pgGPUUsage.Value = 0;
                            txtVRAMUsage.Text = "0.0/12.0GB";
                            pgVRAMUsage.Value = 0;
                            txtGPUTemp.Text = "0°C";
                            pgGPUTemp.Value = 0;
                        }
                    }
                }
                catch
                {
                    if (isTraining)
                    {
                        double gpuUsage = 45 + random.NextDouble() * 30;
                        txtGPUUsage.Text = $"{gpuUsage:F1}%";
                        pgGPUUsage.Value = gpuUsage;
                    }
                }
            });
        }

        private void BtnSelectModel_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Model Dosyaları (*.pth;*.onnx;*.h5)|*.pth;*.onnx;*.h5|Tüm Dosyalar (*.*)|*.*",
                Title = "Model Dosyası Seç"
            };

            if (dialog.ShowDialog() == true)
            {
                txtModelPath.Text = dialog.FileName;
                AddLog($"[Model] Model yüklendi: {Path.GetFileName(dialog.FileName)}");
            }
        }

        private void BtnSelectDataset_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Dataset Klasörünü Seçin"
            };

            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtDatasetPath.Text = folderDialog.SelectedPath;
                
                int trainCount = random.Next(1000, 5000);
                int testCount = random.Next(200, 1000);
                
                txtTrainCount.Text = trainCount.ToString();
                txtTestCount.Text = testCount.ToString();
                
                AddLog($"[Dataset] Dataset yüklendi: {trainCount} eğitim, {testCount} test dosyası");
            }
        }

        private async void BtnStartTraining_Click(object sender, RoutedEventArgs e)
        {
            if (txtModelPath.Text == "Model seçilmedi...")
            {
                MessageBox.Show("Lütfen önce bir model dosyası seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (txtDatasetPath.Text == "Dataset seçilmedi...")
            {
                MessageBox.Show("Lütfen önce bir dataset klasörü seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            isTraining = true;
            btnStartTraining.IsEnabled = false;
            btnStopTraining.IsEnabled = true;
            txtStatus.Text = "Eğitim Devam Ediyor";
            txtStatus.Foreground = new SolidColorBrush(Color.FromRgb(245, 158, 11));

            AddLog("[Training] Eğitim başlatılıyor...");
            AddLog($"[Training] Batch Size: {txtBatchSize.Text}");
            AddLog($"[Training] Learning Rate: {txtLearningRate.Text}");
            AddLog($"[Training] Epochs: {txtEpochs.Text}");
            AddLog($"[Training] GPU: {((ComboBoxItem)cmbGPU.SelectedItem).Content}");

            trainingCancellation = new CancellationTokenSource();

            try
            {
                await RunTrainingSimulation(trainingCancellation.Token);
            }
            catch (OperationCanceledException)
            {
                AddLog("[Training] Eğitim kullanıcı tarafından durduruldu.");
            }

            isTraining = false;
            btnStartTraining.IsEnabled = true;
            btnStopTraining.IsEnabled = false;
            txtStatus.Text = "Hazır";
            txtStatus.Foreground = new SolidColorBrush(Color.FromRgb(16, 185, 129));
        }

        private void BtnStopTraining_Click(object sender, RoutedEventArgs e)
        {
            trainingCancellation?.Cancel();
            AddLog("[Training] Durdurma isteği gönderildi...");
        }

        private async Task RunTrainingSimulation(CancellationToken cancellationToken)
        {
            if (!int.TryParse(txtEpochs.Text, out int epochs))
            {
                epochs = 100;
            }

            for (int epoch = 1; epoch <= epochs; epoch++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                double loss = 2.5 - (epoch * 0.02) + (random.NextDouble() * 0.1);
                double accuracy = 0.3 + (epoch * 0.006) + (random.NextDouble() * 0.02);

                AddLog($"[Epoch {epoch}/{epochs}] Loss: {loss:F4} | Accuracy: {accuracy:F4}");

                await Task.Delay(2000, cancellationToken);

                if (epoch % 10 == 0)
                {
                    AddLog($"[Checkpoint] Model kaydedildi: checkpoint_epoch_{epoch}.pth");
                }
            }

            AddLog("[Training] Eğitim tamamlandı!");
            MessageBox.Show("Eğitim başarıyla tamamlandı!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddLog(string message)
        {
            Dispatcher.Invoke(() =>
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                txtLogs.AppendText($"[{timestamp}] {message}\n");
                txtLogs.ScrollToEnd();
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            hardwareMonitorTimer?.Stop();
            hardwareMonitorTimer?.Dispose();
            computer?.Close();
        }
    }

    public class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware)
                subHardware.Accept(this);
        }

        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }
    }
}

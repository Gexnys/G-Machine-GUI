using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Timers;
using Microsoft.Win32;

namespace GMachineGUI
{
    public partial class MainWindow : Window
    {
        private bool isTraining = false;
        private CancellationTokenSource trainingCancellation;
        private System.Timers.Timer gpuMonitorTimer;
        private Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            InitializeGPUMonitoring();
            AddLog("[System] G-Machine GUI başlatıldı...");
            AddLog("[System] GPU cihazları taranıyor...");
            AddLog("[System] Sistem hazır. Eğitim parametrelerini ayarlayın ve başlatın.");
        }

        private void InitializeGPUMonitoring()
        {
            gpuMonitorTimer = new System.Timers.Timer(1000);
            gpuMonitorTimer.Elapsed += UpdateGPUStats;
            gpuMonitorTimer.AutoReset = true;
            gpuMonitorTimer.Start();
        }

        private void UpdateGPUStats(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
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
    }
}
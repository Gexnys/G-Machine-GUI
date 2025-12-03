# G-Machine-GUI
G-Machine AI – Model Training GUI Application

# Overview
This Windows-based GUI application, developed for G-Machine AI, allows users to train deep learning models in a simple and intuitive way. The application provides model selection, dataset loading, training parameter configuration, and real-time training monitoring in a single platform. Its user-friendly interface makes it accessible even to users without advanced technical knowledge.

# 1. Key Features

# 1-1 Model and Dataset Management 
- Model Selection: Users can select models in .pth, .onnx, .py, and .h5 formats, as well as other file formats.
- Dataset Loading: Users can select a dataset folder; the application automatically shows the number of training and test files.
- The distribution of training and test data is clearly displayed in the interface.

# 1-2 Training Parameters
- Batch Size
- Learning Rate
- Epochs
- GPU Selection: Users can choose from available GPU devices.

# 1-3 Training Control
- Training can be started or stopped with dedicated buttons.
- The status indicator shows the current state of training in real-time.
- Checkpoint files are saved every few epochs to prevent data loss and allow progress recovery.

# 1-4 Training Monitoring
- Real-time simulation of GPU usage, VRAM usage, and temperature.
- The training process logs loss and accuracy for each epoch.
- Logs are displayed in a dedicated, scrollable log panel for easy monitoring.

# 2. User Interface (UI) Features

- Intuitive and clean design: Users can access all functions without confusion.
- Log Panel: Displays step-by-step training process.
- GPU Monitor: Shows GPU usage, VRAM usage, and temperature with numeric and visual indicators.
- Status Indicator: Training is shown in yellow, while ready state is green for clear user feedback.

# 3. Technical Architecture

- Platform: Windows (WPF-based)
- Language: C#
- Asynchronous Training Simulation: Uses async/await to prevent UI freezing during training.
- Cancellable Training: CancellationTokenSource allows users to stop training at any time.
- GPU Monitoring: Uses simulated random data to provide real-time feedback.
- File Management: OpenFileDialog and FolderBrowserDialog are used for selecting models and datasets.

# 4. Advantages

- Ease of Use: Users can train models without deep technical knowledge.
- Training Monitoring: Users can track training progress with detailed logs and GPU statistics.
- Flexible Training Control: Training can be paused or stopped, and checkpoints ensure safe progress.
- Quick Setup: All operations can be performed from a single WPF application.

# 5. Future Improvements
- Model Performance Visualization: Display loss and accuracy graphs during training.
- Multi-GPU Support: Allow usage of multiple GPUs for large models.
- Automatic Checkpoint Management: Archive or remove old checkpoints automatically.

# Summary
The G-Machine AI Model Training GUI simplifies model training through a user-friendly WPF application. With configurable training parameters, GPU monitoring, real-time logging, and checkpointing, users can experience a controlled and secure training process. This interface serves as an effective training management tool for both beginners and experienced users.

# Download link;

- Latest Version 1.5 : https://github.com/Gexnys/G-Machine-GUI/releases/tag/GMachineGUI

- Version 1.0 : https://github.com/Gexnys/G-Machine-GUI/releases/tag/G-MachineGUI
  

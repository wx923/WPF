   M S B A M L   `   `   `  ����A  =WpfApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:WpfApp.ViewModelsWpfApp.ViewModels  5   	       WpfApp.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   .vmclr-namespace:WpfApp.ViewModels     5   	     � Title$  PLC 数据监控��5   	   $	��450��6   $	��800��6'   .��5      � $��20q�6    � RowDefinitions 5   
   �� 5	      $
��Auto �6   �� 5
      $
��Auto �6   �� 5      $��* �6   .+�5   
   �� $��0��6    �� Orientation$ 
Horizontal=�6"   $��0,0,0,10q�6;   .+�5      �� $��连接��6   $	��120��5      $��0,0,10,0q�5      ��5      �� *ConnectCommand+y�5      �� *IsConnected+ $   InverseBoolToVisibilityConverter �� 	Converter# [  �� 5      $��断开连接��6   $	��120��5      $��0,0,10,0q�5      ��5      �� *DisconnectCommand+y�5      �� *IsConnected+  BoolToVisibilityConverter# [ �� 5      $��Center=�5      ��5      �� *ConnectionMessage+5      5      �� 5   
   $��1��6   .+�5      �  � ColumnDefinitions 5      �� 5       $��* �6'   �� 5!      $��* �6'   �� 5"      $��* �6'   .+�5%      �� $��0��6   $��5q�6)    �� Padding$ 10q�64   ���#007ACC$��1q�6W   ._�5&      �� .+�5'      �� $��温度��6$   $��16(�6.   �� 5(      $��24(�5)   "   $
��Bold"�60   ��5(   $   �� *Temperature+ �� StringFormat$ 	{0:F1}°C��5)   0   5*      5+      �� 5-      $��1��6   $��5q�6)   $ 10q�64   ���#007ACC$��1q�6W   ._�5.      �� .+�5/      �� $��压力��6$   $��16(�6.   �� 50      $��24(�51   "   $
��Bold"�60   ��50   $   �� *
Pressure+$ 	{0:F1}MPa��51   0   52      53      �� 55      $��2��6   $��5q�6)   $ 10q�64   ���#007ACC$��1q�6W   ._�56      �� .+�57      �� $��状态��6$   $��16(�6.   �� 58      $��24(�59   "   $
��Bold"�60   ��58   $   �� *Status+  BoolToStatusConverter# [ 59   0   5:      5;      5<      5=      &   System.Windows.Controls.DataGrid  5?   
   $��2��6      AutoGenerateColumns$ False��5A      $��0,20,0,0q�5B      ��5@      �� *HistoricalData+   Columns 5C      0  *System.Windows.Controls.DataGridTextColumn  5D      ,  &System.Windows.Controls.DataGridColumn	   Header$	 时间��6%   
   Width5  /System.Windows.Controls.DataGridLengthConverter$
 * 5F   #   1  +System.Windows.Controls.DataGridBoundColumn   Binding 5E   #   �� *	Timestamp+$ {0:yyyy-MM-dd HH:mm:ss}��5F   #     5G      $	 温度��6%   $
 * 5I   #    5H   #   �� *Temperature+$ 	{0:F1}°C��5I   #     5J      $	 压力��6%   $
 * 5L   #    5K   #   �� *
Pressure+$ 	{0:F1}MPa��5L   #     5M      $	 状态��6%   $
 * 5O   #    5N   #   �� *Status+# [ 5O   #   5Q      5R      5S      
   M S B A M L   `   `   `  ����   DudeBot"    CustomsForgeLogin.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     
����ح�  � Title$  CustomsForgeLogin��$	��180��$	��300��.���  � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � � ColumnDefinitions �� $	��50* ��� $	��50* �.+��� $��Center=�$
��Bold"�$��12(�$'��!Connect DudeBot With CustomsForge�� � Cursor$
 None��$��Center=�$��0��$��0��$��2���� $��Center=�$��12(�$��1��$��
Username: ��$��0��$��Right=��� -    ��Name$ 
cfUserName��$��12(�$��1��$��1������ *requestSystem.cfUserName+ �� UpdateSourceTrigger$ PropertyChanged=��� $��Center=�$��12(�$��2��$��
Password: ��$��0��$��Right=�G� -    G�Name$ 
cfPassword��$��12(�$��2��$��1�� G� PasswordChar$ *�� G� Password$  ���� $��Center=�$��12(�$��3��$4��.*Username and password are both case sensitive��$��0��$��2��$��Center=��� $��Center=�$��12(�$��4��$6��0*I am not liable if your CF account gets deleted��$��0��$��2��$��Center=��� -   $��12(�   SquareButtonStyle#���   $��Connect��$��0��$��5��$��2��
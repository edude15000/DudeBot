   M S B A M L   `   `   `  ����  DudeBot"    CustomsForgeLogin.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   
����ح�  � Title$  CustomsForgeLogin��5   	   $	��180��6#   $	��300��60   .��5	      �  � RowDefinitions 5
   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6    � ColumnDefinitions 5   
   �� 5      $	��50* �6   �� 5      $	��50* �6   .+�5   
   �� $��Center=�6   $
��Bold"�61   $��12(�6C   $'��!Connect DudeBot With CustomsForge��6Q    � Cursor$
 None��6z   $��Center=�6�   $��0��6�   $��0��6�   $��2��6�   �� 5   
   $��Center=�6   $��12(�60   $��1��6>   $��
Username: ��6K   $��0��6]   $��Right=�6m   �� 5   
   -   5         ��Name$ 
cfUserName��5      $��12(�6&   $��1��64   $��1��6A   ��6Q   �� *requestSystem.cfUserName+ �� UpdateSourceTrigger$ PropertyChanged=��� 5   
   $��Center=�6   $��12(�60   $��2��6>   $��
Password: ��6K   $��0��6]   $��Right=�6m   G� 5   
   -   5         G�Name$ 
cfPassword��5      $��12(�6*   $��2��68   $��1��6E    G� PasswordChar$ *��6U    G� Password$  ��6f   �� 5   
   $��Center=�6   $��12(�60   $��3��6>   $4��.*Username and password are both case sensitive��6K   $��0��6�   $��2��6�   $��Center=�6�   �� 5   
   $��Center=�6   $��12(�60   $��4��6>   $6��0*I am not liable if your CF account gets deleted��6K   $��0��6�   $��2��6�   $��Center=�6�   �� 5    
   -   5        $��12(�5          SquareButtonStyle#���   6   $��Connect��6K   $��0��6r   $��5��6�   $��2��6�   5"      5#      
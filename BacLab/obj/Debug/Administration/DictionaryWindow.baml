   M S B A M L   `   `   `  ����A  =BacLab, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:BacLab.DictionaryBacLab.Dictionary  5      ,    &BacLab.Administration.DictionaryWindow   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      T PMahApps.Metro, Version=1.3.0.0, Culture=neutral, PublicKeyToken=f4fb5a3c4d1e5b4f;controls,http://metro.mahapps.com/winfx/xaml/controls  5      V RMaterialDesignThemes.Wpf, Version=2.4.0.1044, Culture=neutral, PublicKeyToken=nullX TMaterialDesignThemes.MahApps, Version=0.0.11.0, Culture=neutral, PublicKeyToken=nullHmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes   5      2
Dictionaryclr-namespace:BacLab.Dictionary   5      (  "MahApps.Metro.Controls.MetroWindow    WindowTransitionsEnabled  .     MaterialDesignBody#���   5        MaterialDesignPaper#���  5	        AccentColorBrush   	GlowBrush# �  5
        DefaultFont#��[ 5       � Icon$+ %/BacLab;component/Microscope-icon.ico��5       � Title$ BacLab��5       � WindowStartupLocation$ CenterScreen=�5       � WindowState$ 	Maximized=�5       � 
ResizeMode$ 	CanResize=�6/   .��5      �  � ColumnDefinitions 5   
   �� 5      $
��auto �6   �� 5      .+�5   
   (  "MaterialDesignThemes.Wpf.ColorZone  $��0��6#   +  %MaterialDesignThemes.Wpf.ShadowAssist   ShadowDepth$ Depth2=�5   #   	   Mode$	 Light=�5   #   $��Stretch=�60   .��5      �� .��5      �� .+�5      �� -   5        
 ��Name$

 x_AB��5      $��Антибіотики��6+   $��5q�6A     MaterialDesignRaisedButton#���  5      $ Depth4=�5      $��Arial Rounded MT Bold)�5    *   $��18(�6M   �� 5!      -   5        $
 	x_ABGroup��5!      $)��#Групи антибіотиків��60   $��5q�6M   #���  5"      $ Depth4=�5#      $��Arial Rounded MT Bold)�5$   *   $��18(�6M   �� 5%      -   5        $
 x_CategoryPatient��5%      $+��%Категорії пацієнтів��69   $��5q�6W   #���  5&      $ Depth4=�5'      $��Arial Rounded MT Bold)�5(   *   $��18(�6M   �� 5)      -   5        $
 x_Department��5)      $��Відділення��64   $��5q�6I   #���  5*      $ Depth4=�5+      $��Arial Rounded MT Bold)�5,   *   $��18(�6M   �� 5-      -   5        $
 x_Diagnosis��5-      $��Діагнози��63   $��5q�6F   #���  5.      $ Depth4=�5/      $��Arial Rounded MT Bold)�50   *   $��18(�6M   �� 51      -   5        $
 x_DiagnosisGroup��51      $#��Групи діагнозів��68   $��5q�6R   #���  52      $ Depth4=�53      $��Arial Rounded MT Bold)�54   *   $��18(�6M   �� 55      -   5        $
 
x_District��55      $��Райони міста��62   $��5q�6I   #���  56      $ Depth4=�57      $��Arial Rounded MT Bold)�58   *   $��18(�6M   �� 59      -   5        $
 	x_Finance��59      $��Фінаси��61   $��5q�6B   #���  5:      $ Depth4=�5;      $��Arial Rounded MT Bold)�5<   *   $��18(�6M   �� 5=      -	   5        $
 x_GroupOfStudy��5=      $%��Групи досліджень��66   $��5q�6Q   #���  5>      $ Depth4=�5?      $��Arial Rounded MT Bold)�5@   *   $��18(�6M   �� 5A      -
   5        $
 x_Institution��5A      $��Лікарні��65   $��5q�6G   #���  5B      $ Depth4=�5C      $��Arial Rounded MT Bold)�5D   *   $��18(�6M   �� 5E      -   5        $
 
x_Material��5E      $��Матеріал��62   $��5q�6E   #���  5F      $ Depth4=�5G      $��Arial Rounded MT Bold)�5H   *   $��18(�6M   �� 5I      -   5        $
 x_Medium��5I      $��Середовища��6/   $��5q�6D   #���  5J      $ Depth4=�5K      $��Arial Rounded MT Bold)�5L   *   $��18(�6M   �� 5M      -   5        $

 x_MO��5M      $"��Мікроорганізми��6+   $��5q�6D   #���  5N      $ Depth4=�5O      $��Arial Rounded MT Bold)�5P   *   $��18(�6M   �� 5Q      -   5        $
 	x_MOGenus��5Q      $��Род МО��60   $��5q�6A   #���  5R      $ Depth4=�5S      $��Arial Rounded MT Bold)�5T   *   $��18(�6M   �� 5U      -   5        $
 	x_MOGroup��5U      $��Групи МО��60   $��5q�6C   #���  5V      $ Depth4=�5W      $��Arial Rounded MT Bold)�5X   *   $��18(�6M   �� 5Y      -   5        $
 x_Morphology��5Y      $��Морфологія��63   $��5q�6H   #���  5Z      $ Depth4=�5[      $��Arial Rounded MT Bold)�5\   *   $��18(�6M   �� 5]      -   5        $
 x_Tests��5]      $��
Тести��6.   $��5q�6>   #���  5^      $ Depth4=�5_      $��Arial Rounded MT Bold)�5`   *   $��18(�6M   �� 5a      -   5        $
 x_NameTestSet��5a      $(��"Назви біохім рядів��64   $��5q�6Q   #���  5b      $ Depth4=�5c      $��Arial Rounded MT Bold)�5d   *   $��18(�6M   �� 5e      -   5        $
 x_Olkenicky��5e      $1��+Варіанти Олькіницького��62   $��5q�6S   #���  5f      $ Depth4=�5g      $��Arial Rounded MT Bold)�5h   *   $��18(�6M   �� 5i      -   5        $
 	x_Purpose��5i      $%��Мета дослідження��60   $��5q�6K   #���  5j      $ Depth4=�5k      $��Arial Rounded MT Bold)�5l   *   $��18(�6M   �� 5m      -   5        $
 x_StaffCategory��5m      $/��)Категорії працівників��66   $��5q�6V   #���  5n      $ Depth4=�5o      $��Arial Rounded MT Bold)�5p   *   $��18(�6M   �� 5q      -   5        $
 x_Staff��5q      $��Працівники��6.   $��5q�6C   #���  5r      $ Depth4=�5s      $��Arial Rounded MT Bold)�5t   *   $��18(�6M   �� 5u      -   5        $
 	x_TestRes��5u      $'��!Результати тестів��60   $��5q�6L   #���  5v      $ Depth4=�5w      $��Arial Rounded MT Bold)�5x   *   $��18(�6M   �� 5y      -   5        $
 x_TypeColny��5y      $��Типи колоній��62   $��5q�6I   #���  5z      $ Depth4=�5{      $��Arial Rounded MT Bold)�5|   *   $��18(�6M   5}      5~      5      #  MaterialDesignThemes.Wpf.Card  5�   
   -   5          Name$ x_Card��5�      $��30,5,5,5q�6.   $��1��6A   $��Stretch=�6Q   5�      5�      
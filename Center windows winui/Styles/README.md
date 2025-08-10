The FontSizes dictionary exposes a set of typographic resources that you can use with {ThemeResource …} or {StaticResource …} in your styles and controls.

Resource	Size (px)
FontSize10	10
FontSize12	12
FontSize14	14
FontSize16	16
FontSize18	18
FontSize20	20
FontSize22	22
FontSize24	24
FontSize26	26
FontSize28	28

<!-- Usage example -->
  <Style x:Key="DataLabelTextStyle"
         TargetType="TextBlock"
         BasedOn="{StaticResource BodyTextBlockStyle}">
    <Setter Property="FontSize"    Value="{ThemeResource FontSize16}" />
    <Setter Property="Foreground"  Value="#FF666666" />
    <Setter Property="FontWeight"  Value="SemiBold" />
    <Setter Property="VerticalAlignment" Value="Center" />
  </Style>
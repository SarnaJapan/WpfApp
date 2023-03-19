# WpfApp : C#/WPF Othello

Othello(Reversi) implementation for C#/WPF learning.

The goal is to make it as simple and easy to understand as possible with MVVM/XAML architecture.

## [Features]
- MVVM library NOT used.
- Flip animation by XAML.
- Bitboard implementation of the Othello board.
- The algorithm for the sample player is Monte Carlo and MCTS.

Suggestions for improvement are welcome!

![WpfApp.png](/WpfApp.png)

## [開発メモ]
- プレイヤーは外部プラグイン対応済み。
- プレイヤー処理部はマルチスレッド対応済み。
- 現状では.NET Framework 4.6.1対応。次版では.NET6対応予定。
- Accord.NetやKelpNetによる機械学習プレイヤーを実装予定。
　

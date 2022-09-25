# WpfApp : C#/WPF Othello

Othello(Reversi) implementation for C#/WPF learning.

The goal is to make it as simple and easy to understand as possible with MVVM/XAML architecture.

## [Features]
- MVVM library NOT used.
- Flip animation by XAML.
- Bitboard implementation of the Othello board.
- The algorithm for the sample player is Monte Carlo and MCTS.

Suggestions for improvement are welcome!

## [開発メモ]
- 機械学習オセロのベースとなるプログラムを作成。これを基にAccord.NetやKelpNetによるプレイヤーを実装中。
- 現状.NET Framework 4.6.1対応。別途.NET6対応実装中。
- プレイヤーは外部プラグイン対応済み。
- プレイヤー処理部はマルチスレッド対応済み。

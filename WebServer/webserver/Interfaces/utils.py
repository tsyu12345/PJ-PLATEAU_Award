from enum import Enum

class PrintColor(Enum):
    # いくつかの基本的なANSIエスケープコード
    RED = "\033[31m"  # 赤色
    GREEN = "\033[32m" # 緑色
    YELLOW = "\033[33m" # 黄色
    BLUE = "\033[34m" # 青色
    MAGENTA = "\033[35m" # マゼンタ
    CYAN = "\033[36m" # シアン
    RESET = "\033[0m"  # リセット（デフォルトの色に戻す）
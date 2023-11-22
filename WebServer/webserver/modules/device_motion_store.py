from ..Interfaces.activity_interface import ActivityData
class DeviceMotion:
    """
    各ユーザー毎(ローカル端末毎)に、デバイスの動きを記録するクラス
    """

    def __init__(self):
        self.__DATA_BASE: dict[str, float] = {}
    
    def renew(self, data: ActivityData):
        """デバイスの動きを更新する"""
        self.__DATA_BASE[data.user_id] = data.strength
    
    def get_strength(self, user_id) -> float | None:
        try:
            return self.__DATA_BASE[user_id]
        except KeyError:
            return 0.0
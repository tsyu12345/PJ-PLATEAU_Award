class DeviceMortion:
    """
    各ユーザー毎に、デバイスの動きを記録するクラス
    """
    def __init__(self, user_id: str):
        self.__user_id = user_id
        self.__strength = 0.0
    
    def renew(self, strength: float):
        self.__strength = strength
    
    def get_strength(self):
        return self.__strength
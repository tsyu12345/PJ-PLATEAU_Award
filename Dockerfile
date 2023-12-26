#dockerイメージを指定。
FROM python:3.11.4-slim
#パッケージのアップデート&curlをダウンロード
RUN apt-get update && apt-get -y install curl
#環境変数を設定。
ENV PATH /root/.local/bin/:$PATH
#poetry(pythonのパッケージ管理ツール)をダウンロード。
RUN curl -sSL https://install.python-poetry.org | python3 -
#コンテナ内での作業ディレクトリを指定。
WORKDIR root

#コンテナ内にディレクトリを作成。
RUN mkdir app
#ロ-カルファイルをコンテナ内のroot配下にコピー。
COPY pyproject.toml poetry.lock ./
COPY server_config.json ./
COPY WebServer/webserver app/

#poetryの仮想化を無効。
RUN poetry config virtualenvs.create false
#pyproject.tomlで設定されているパッケージをインストール。
RUN poetry install --only main
#コンテナ起動時に実行するコマンドを指定。
ENTRYPOINT ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8080"]
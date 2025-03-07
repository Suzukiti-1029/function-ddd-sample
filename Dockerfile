# 基本イメージ (.NET 6 SDKを使用)
FROM mcr.microsoft.com/dotnet/sdk:6.0

# 必要に応じてツールやエクステンションのインストールを行う
RUN apt-get update && apt-get install -y \
  # 例: F#に関連するツールがあればここでインストール
  && rm -rf /var/lib/apt/lists/*

# 作業ディレクトリの設定
WORKDIR /workspace

# コンテナ内で非特権ユーザーを作成する場合
# RUN useradd -m devuser \
#     && chown -R devuser /workspace
# USER devuser

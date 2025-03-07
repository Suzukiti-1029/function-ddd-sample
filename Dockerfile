# 基本イメージ (.NET 8 SDKを使用)
FROM mcr.microsoft.com/dotnet/sdk:8.0

RUN apt-get update && apt-get install -y \
  # git を追加でインストール
  git \
  && rm -rf /var/lib/apt/lists/*

RUN mkdir -p /workspace/function-ddd-sample

# 作業ディレクトリの設定
WORKDIR /workspace/function-ddd-sample

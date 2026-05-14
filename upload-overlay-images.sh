#!/bin/bash

# Upload overlay images and JSON files to Cloudflare R2 using rclone
# Uses local rclone.conf file in the same directory
#
# Usage: ./upload-overlay-images.sh <all|images|json|sprites> [game1] [game2] ...
# Examples:
#   ./upload-overlay-images.sh all           # Sync everything for all games
#   ./upload-overlay-images.sh images        # Sync only images for all games
#   ./upload-overlay-images.sh sprites       # Sync only sprite files for all games
#   ./upload-overlay-images.sh json ets2     # Sync only JSON for ets2
#   ./upload-overlay-images.sh all ets2 ats  # Sync everything for ets2 and ats

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CONFIG_FILE="$SCRIPT_DIR/rclone.conf"

# Parse arguments
SYNC_MODE="${1:-all}"
shift

# Validate sync mode
if [[ ! "$SYNC_MODE" =~ ^(all|images|json|sprites)$ ]]; then
    echo "Error: Invalid sync mode '$SYNC_MODE'"
    echo "Usage: $0 <all|images|json|sprites> [game1] [game2] ..."
    exit 1
fi

# Determine which games to sync
if [ $# -gt 0 ]; then
    GAMES=("$@")
else
    GAMES=("ets2" "ats" "promods")
fi

echo "Sync mode: $SYNC_MODE"
echo "Games: ${GAMES[*]}"
echo ""

# Load environment variables from .env file
if [ -f "$SCRIPT_DIR/.env" ]; then
    # Strip carriage returns and load variables (handles Windows CRLF line endings)
    while IFS= read -r line || [ -n "$line" ]; do
        # Remove carriage return if present
        line=$(echo "$line" | tr -d '\r')
        # Skip comments and empty lines
        if [[ ! "$line" =~ ^# && -n "$line" ]]; then
            export "$line"
        fi
    done < "$SCRIPT_DIR/.env"
fi

# Check if config file exists
if [ ! -f "$CONFIG_FILE" ]; then
    echo "Error: rclone.conf not found in $SCRIPT_DIR"
    echo "Please create an rclone.conf file with your R2 credentials"
    exit 1
fi

# Remote name as configured in rclone.conf (default: r2)
REMOTE_NAME="${RCLONE_REMOTE:-r2}"
BUCKET_NAME="${R2_BUCKET}"

if [ -z "$BUCKET_NAME" ]; then
    echo "Error: R2_BUCKET not set in .env file"
    exit 1
fi

echo "Starting sync of map data to Cloudflare R2..."
echo "Using config: $CONFIG_FILE"
echo "Remote: $REMOTE_NAME"
echo "Bucket: $BUCKET_NAME"

# Function to sync overlay images for a game
sync_overlay_images() {
    local game=$1
    local overlay_dir="$SCRIPT_DIR/map_data/$game/overlay_images"
    
    if [ ! -d "$overlay_dir" ]; then
        echo "Warning: $game overlay_images directory not found"
        return
    fi
    
    echo ""
    echo "==> Syncing $game overlay images..."
    echo "Source: $overlay_dir"
    echo "Destination: $REMOTE_NAME:$BUCKET_NAME/map_data/$game/overlay_images"
    
    rclone sync \
        --config "$CONFIG_FILE" \
        --progress \
        --transfers 4 \
        --checkers 8 \
        --s3-upload-concurrency 2 \
        "$overlay_dir" \
        "$REMOTE_NAME:$BUCKET_NAME/map_data/$game/overlay_images"
    
    if [ $? -eq 0 ]; then
        echo "✓ $game overlay images synced successfully"
    else
        echo "✗ Failed to sync $game overlay images"
        exit 1
    fi
}

# Function to sync JSON files for a game
sync_json_files() {
    local game=$1
    local game_dir="$SCRIPT_DIR/map_data/$game"
    
    if [ ! -d "$game_dir" ]; then
        echo "Warning: $game directory not found"
        return
    fi
    
    echo ""
    echo "==> Syncing $game JSON files..."
    echo "Source: $game_dir"
    echo "Destination: $REMOTE_NAME:$BUCKET_NAME/map_data/$game/"
    
    rclone sync \
        --config "$CONFIG_FILE" \
        --progress \
        --transfers 4 \
        --checkers 8 \
        --s3-upload-concurrency 2 \
        --filter "+ *.json" \
        --filter "- *" \
        "$game_dir/" \
        "$REMOTE_NAME:$BUCKET_NAME/map_data/$game/"
    
    if [ $? -eq 0 ]; then
        echo "✓ $game JSON files synced successfully"
    else
        echo "✗ Failed to sync $game JSON files"
        echo "Check your R2 bucket permissions for write access"
        exit 1
    fi
}

# Function to sync sprite files for a game
sync_sprites() {
    local game=$1
    local sprites_dir="$SCRIPT_DIR/map_data/$game/sprites"
    
    if [ ! -d "$sprites_dir" ]; then
        echo "Warning: $game sprites directory not found"
        echo "Run: node generate-sprites.js $game"
        return
    fi
    
    echo ""
    echo "==> Syncing $game sprite files..."
    echo "Source: $sprites_dir"
    echo "Destination: $REMOTE_NAME:$BUCKET_NAME/map_data/$game/sprites"
    
    rclone sync \
        --config "$CONFIG_FILE" \
        --progress \
        --transfers 4 \
        --checkers 8 \
        --s3-upload-concurrency 2 \
        --header-upload "Cache-Control: public, max-age=300" \
        --header-upload "x-amz-meta-uploaded: $(date +%s)" \
        "$sprites_dir" \
        "$REMOTE_NAME:$BUCKET_NAME/map_data/$game/sprites"
    
    if [ $? -eq 0 ]; then
        echo "✓ $game sprite files synced successfully"
    else
        echo "✗ Failed to sync $game sprite files"
        exit 1
    fi
}

# Sync overlay images for selected games
if [[ "$SYNC_MODE" == "all" || "$SYNC_MODE" == "images" ]]; then
    for game in "${GAMES[@]}"; do
        sync_overlay_images "$game"
    done
fi

# Sync JSON files for selected games
if [[ "$SYNC_MODE" == "all" || "$SYNC_MODE" == "json" ]]; then
    for game in "${GAMES[@]}"; do
        sync_json_files "$game"
    done
fi

# Sync sprite files for selected games
if [[ "$SYNC_MODE" == "all" || "$SYNC_MODE" == "sprites" ]]; then
    for game in "${GAMES[@]}"; do
        sync_sprites "$game"
    done
fi

echo ""
echo "==> Sync complete!"

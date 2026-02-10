#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']

apiUrl = "api/AlertChannel/org/#{orgId}/action/AddAlertChannel"
param = {
  Type: "Discord",
  Status: "Enabled",
  ChannelName: "Discord Alert Channel #2",
  Description: "This is a test Discord alert channel #2",
  DiscordWebhookUrl: ENV['DISCORD_WEBHOOK_URL'],
  Tags: "testing",
}

result = make_request(:post, apiUrl, param)

json = result.to_json
puts(json)

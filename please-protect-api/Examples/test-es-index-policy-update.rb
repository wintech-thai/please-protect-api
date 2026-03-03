#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']

apiUrl = "api/Es/org/#{orgId}/action/UpdateIndexPolicy"
param = {
  warmDayCount: 7,
  coldDayCount: 15,
  deleteDayCount: 30
}

result = make_request(:post, apiUrl, param)

json = result.to_json
puts(json)

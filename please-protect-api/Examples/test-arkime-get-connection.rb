#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']

# -------------------------
# Arkime query
# -------------------------

# ตัวอย่าง filter (Arkime expression)
expression = "protocols == ssh" # อันนี้ จะอยู่ตรงช่อง search

# time range (ย้อนหลัง 1 ชั่วโมง)
start_ts = (Time.now - 3600).to_i
end_ts   = Time.now.to_i

queryStr = URI.encode_www_form(
  #expression: expression,
  startTime: start_ts,
  stopTime: end_ts,
  date: 1,      # 1 = custom time
  length: 10    # limit result
)

# -------------------------
# Proxy -> Arkime API
# -------------------------
apiUrl = "api/Proxy/org/#{orgId}/action/Arkime/api/sessions?#{queryStr}"
apiUrl = "api/Proxy/org/#{orgId}/action/Arkime/api/fields" # อันนี้ใช้เป็นตัวบอกว่ามี field อะไรบ้างใช้ในการคิวรี่ได้

param = nil

result = make_request(:get, apiUrl, param)

puts JSON.pretty_generate(result)

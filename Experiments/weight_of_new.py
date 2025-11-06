import streamlit as st
import pandas as pd

# 初始化 session_state
if "avgPerHit" not in st.session_state:
    st.session_state.avgPerHit = None
if "history" not in st.session_state:
    st.session_state.history = []  # list of (step, avg, new_value)

# 滑动条调整 weight_of_new
weight = st.slider("Weight of new", 0.0, 1.0, 0.1)

# 输入框：使用 form 来处理伤害输入
with st.form("damage_form"):
    damage = st.number_input("Enter damage", min_value=0.0, max_value=9999.0, step=0.1)
    submitted = st.form_submit_button("Submit Hit")
    if submitted and 0 < damage < 9999:
        if st.session_state.avgPerHit is None:
            st.session_state.avgPerHit = damage
        else:
            st.session_state.avgPerHit = (st.session_state.avgPerHit * (1 - weight)) + (damage * weight)
        st.session_state.history.append((len(st.session_state.history) + 1, st.session_state.avgPerHit, damage))

# 连发功能：输入多个伤害值，用逗号分隔
burst_input = st.text_input("Enter multiple damages (comma-separated)", "")
if st.button("Send Burst"):
    if burst_input:
        damages = [float(x.strip()) for x in burst_input.split(",") if x.strip()]
        valid_damages = [d for d in damages if 0 < d < 9999]
        for dmg in valid_damages:
            if st.session_state.avgPerHit is None:
                st.session_state.avgPerHit = dmg
            else:
                st.session_state.avgPerHit = (st.session_state.avgPerHit * (1 - weight)) + (dmg * weight)
            st.session_state.history.append((len(st.session_state.history) + 1, st.session_state.avgPerHit, dmg))
        st.success(f"Processed {len(valid_damages)} valid damages.")

# 重置按钮
if st.button("Reset"):
    st.session_state.avgPerHit = None
    st.session_state.history = []

# 折线图表：展示 avgPerHit 和 new_value 的变化
if st.session_state.history:
    df = pd.DataFrame(st.session_state.history, columns=["Step", "avgPerHit", "new_value"])
    st.line_chart(df.set_index("Step")[["avgPerHit", "new_value"]])
else:
    st.write("No data yet. Enter damage to start.")
